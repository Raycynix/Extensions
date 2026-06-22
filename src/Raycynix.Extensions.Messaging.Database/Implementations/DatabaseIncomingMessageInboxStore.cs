using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Database;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Configurations;
using Raycynix.Extensions.Messaging.Database.Models;

namespace Raycynix.Extensions.Messaging.Database.Implementations;

/// <summary>
/// Persists inbox state for incoming messages in the configured database.
/// </summary>
internal sealed class DatabaseIncomingMessageInboxStore(
    RaycynixDatabaseContext databaseContext,
    MessagingConfiguration configuration,
    ILogger<DatabaseIncomingMessageInboxStore>? logger = null) : IIncomingMessageInboxStore
{
    /// <inheritdoc />
    public async Task<bool> TryBeginProcessingAsync(IncomingTransportMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var set = databaseContext.Set<MessagingInboxEntryEntity>();
        var existing = set.Local.SingleOrDefault(entry => entry.MessageId == message.MessageId) ??
                       await set.SingleOrDefaultAsync(entry => entry.MessageId == message.MessageId, cancellationToken)
                           .ConfigureAwait(false);

        if (existing is null)
        {
            set.Add(new MessagingInboxEntryEntity
            {
                MessageId = message.MessageId,
                Destination = message.Destination,
                Status = (int)IncomingMessageInboxStatus.Processing,
                UpdatedAt = DateTimeOffset.UtcNow
            });

            try
            {
                await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                logger?.LogDebug("Created inbox processing entry. Destination={Destination}.", message.Destination);
                return true;
            }
            catch (DbUpdateException)
            {
                // Another worker may have inserted the same inbox row concurrently.
                var current = await GetCurrentEntryAsync(message.MessageId, cancellationToken).ConfigureAwait(false);
                if (current is not null)
                {
                    return (IncomingMessageInboxStatus)current.Status switch
                    {
                        IncomingMessageInboxStatus.Processing => await TryReclaimStaleProcessingEntryAsync(
                                current,
                                message,
                                cancellationToken)
                            .ConfigureAwait(false),
                        IncomingMessageInboxStatus.Processed => false,
                        IncomingMessageInboxStatus.Failed => await TryResumeFailedEntryAsync(
                                message,
                                cancellationToken)
                            .ConfigureAwait(false),
                        _ => false
                    };
                }

                throw;
            }
        }

        return await HandleExistingEntryAsync(existing, message, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task MarkProcessedAsync(IncomingTransportMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var set = databaseContext.Set<MessagingInboxEntryEntity>();
        var entry = set.Local.SingleOrDefault(current => current.MessageId == message.MessageId) ??
                    await set.SingleOrDefaultAsync(current => current.MessageId == message.MessageId, cancellationToken)
                        .ConfigureAwait(false);

        if (entry is null)
        {
            throw new InvalidOperationException($"Inbox entry '{message.MessageId}' was not found.");
        }

        entry.Status = (int)IncomingMessageInboxStatus.Processed;
        entry.UpdatedAt = DateTimeOffset.UtcNow;
        entry.Error = null;
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        logger?.LogDebug("Marked inbox message as processed. Destination={Destination}.", message.Destination);
    }

    /// <inheritdoc />
    public async Task MarkFailedAsync(
        IncomingTransportMessage message,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(exception);

        var set = databaseContext.Set<MessagingInboxEntryEntity>();
        var entry = set.Local.SingleOrDefault(current => current.MessageId == message.MessageId) ??
                    await set.SingleOrDefaultAsync(current => current.MessageId == message.MessageId, cancellationToken)
                        .ConfigureAwait(false);

        if (entry is null)
        {
            throw new InvalidOperationException($"Inbox entry '{message.MessageId}' was not found.");
        }

        entry.Status = (int)IncomingMessageInboxStatus.Failed;
        entry.UpdatedAt = DateTimeOffset.UtcNow;
        entry.Error = exception.Message;
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        logger?.LogDebug("Marked inbox message as failed. Destination={Destination}, ExceptionType={ExceptionType}.",
            message.Destination, exception.GetType().FullName);
    }

    /// <inheritdoc />
    public async Task<IncomingMessageInboxEntry?> GetAsync(string messageId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);

        var entry = databaseContext.ChangeTracker.Entries<MessagingInboxEntryEntity>()
            .Where(current => current.Entity.MessageId == messageId)
            .Select(current => current.Entity)
            .SingleOrDefault() ?? await databaseContext.Set<MessagingInboxEntryEntity>()
            .AsNoTracking()
            .SingleOrDefaultAsync(current => current.MessageId == messageId, cancellationToken)
            .ConfigureAwait(false);

        return entry is null
            ? null
            : new IncomingMessageInboxEntry
            {
                MessageId = entry.MessageId,
                Destination = entry.Destination,
                Status = (IncomingMessageInboxStatus)entry.Status,
                UpdatedAt = entry.UpdatedAt,
                Error = entry.Error
            };
    }

    /// <summary>
    /// Handles an already existing inbox entry according to its current processing state.
    /// </summary>
    /// <param name="existing">The existing inbox row.</param>
    /// <param name="message">The incoming transport message being processed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> when processing may continue; otherwise, <see langword="false"/>.</returns>
    private async Task<bool> HandleExistingEntryAsync(
        MessagingInboxEntryEntity existing,
        IncomingTransportMessage message,
        CancellationToken cancellationToken)
    {
        if ((IncomingMessageInboxStatus)existing.Status == IncomingMessageInboxStatus.Processed)
        {
            logger?.LogDebug("Inbox message skipped because it is already processed. Destination={Destination}.",
                message.Destination);
            return false;
        }

        if ((IncomingMessageInboxStatus)existing.Status == IncomingMessageInboxStatus.Processing)
        {
            return await TryReclaimStaleProcessingEntryAsync(existing, message, cancellationToken)
                .ConfigureAwait(false);
        }

        return await TryResumeFailedEntryAsync(message, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Attempts to move a failed inbox row back to the processing state using optimistic concurrency.
    /// </summary>
    /// <param name="message">The incoming transport message being resumed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> when the failed entry was resumed by the current worker.</returns>
    private async Task<bool> TryResumeFailedEntryAsync(
        IncomingTransportMessage message,
        CancellationToken cancellationToken)
    {
        var set = databaseContext.Set<MessagingInboxEntryEntity>();
        var entry = set.Local.SingleOrDefault(current => current.MessageId == message.MessageId) ??
                    await set.SingleOrDefaultAsync(current => current.MessageId == message.MessageId, cancellationToken)
                        .ConfigureAwait(false);

        if (entry is null)
        {
            return false;
        }

        if ((IncomingMessageInboxStatus)entry.Status is IncomingMessageInboxStatus.Processing
            or IncomingMessageInboxStatus.Processed)
        {
            return false;
        }

        entry.Status = (int)IncomingMessageInboxStatus.Processing;
        entry.UpdatedAt = DateTimeOffset.UtcNow;
        entry.Error = null;

        try
        {
            await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            logger?.LogDebug("Resumed failed inbox entry. Destination={Destination}.", message.Destination);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            databaseContext.Entry(entry).State = EntityState.Unchanged;
            logger?.LogDebug("Failed inbox entry resume lost due to optimistic concurrency. Destination={Destination}.",
                message.Destination);
            return false;
        }
    }

    /// <summary>
    /// Loads the latest persisted inbox row without using the current change tracker state.
    /// </summary>
    /// <param name="messageId">The identifier of the inbox row to load.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The latest persisted inbox entity, or <see langword="null"/> when none exists.</returns>
    private async Task<MessagingInboxEntryEntity?> GetCurrentEntryAsync(
        string messageId,
        CancellationToken cancellationToken)
    {
        return await databaseContext.Set<MessagingInboxEntryEntity>()
            .AsNoTracking()
            .SingleOrDefaultAsync(current => current.MessageId == messageId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Attempts to reclaim a stale processing lease using optimistic concurrency on the tracked inbox entity.
    /// </summary>
    /// <param name="entry">The tracked inbox row that may be stale.</param>
    /// <param name="message">The incoming message requesting the lease.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> when the current worker successfully reclaimed the stale lease.</returns>
    private async Task<bool> TryReclaimStaleProcessingEntryAsync(
        MessagingInboxEntryEntity entry,
        IncomingTransportMessage message,
        CancellationToken cancellationToken)
    {
        if (!IsProcessingLeaseStale(entry.UpdatedAt))
        {
            logger?.LogDebug("Inbox processing lease is still active. Destination={Destination}.", message.Destination);
            return false;
        }

        entry.Destination = message.Destination;
        entry.UpdatedAt = DateTimeOffset.UtcNow;
        entry.Error = null;

        try
        {
            await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            logger?.LogDebug("Reclaimed stale inbox processing lease. Destination={Destination}.", message.Destination);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            databaseContext.Entry(entry).State = EntityState.Unchanged;
            logger?.LogDebug(
                "Inbox processing lease reclaim lost due to optimistic concurrency. Destination={Destination}.",
                message.Destination);
            return false;
        }
    }

    /// <summary>
    /// Determines whether the processing lease stored in the inbox row has expired.
    /// </summary>
    /// <param name="updatedAt">The timestamp of the last lease update.</param>
    /// <returns><see langword="true"/> when the lease is stale and may be reclaimed.</returns>
    private bool IsProcessingLeaseStale(DateTimeOffset updatedAt)
    {
        return DateTimeOffset.UtcNow - updatedAt >= configuration.IncomingProcessing.ProcessingLeaseTimeout;
    }
}
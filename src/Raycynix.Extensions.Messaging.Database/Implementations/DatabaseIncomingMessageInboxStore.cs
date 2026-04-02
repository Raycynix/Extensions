using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.Implementations;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Configurations;
using Raycynix.Extensions.Messaging.Database.Models;

namespace Raycynix.Extensions.Messaging.Database.Implementations;

/// <summary>
/// Persists inbox state for incoming messages in the configured database.
/// </summary>
internal sealed class DatabaseIncomingMessageInboxStore(
    DatabaseContext databaseContext,
    MessagingConfiguration configuration) : IIncomingMessageInboxStore
{
    /// <inheritdoc />
    public async Task<bool> TryBeginProcessingAsync(IncomingTransportMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var set = databaseContext.Set<MessagingInboxEntryEntity>();
        var existing = set.Local.SingleOrDefault(entry => entry.MessageId == message.MessageId) ??
            await set.SingleOrDefaultAsync(entry => entry.MessageId == message.MessageId, cancellationToken).ConfigureAwait(false);

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
    public async Task MarkProcessedAsync(IncomingTransportMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var set = databaseContext.Set<MessagingInboxEntryEntity>();
        var entry = set.Local.SingleOrDefault(current => current.MessageId == message.MessageId) ??
            await set.SingleOrDefaultAsync(current => current.MessageId == message.MessageId, cancellationToken).ConfigureAwait(false);

        if (entry is null)
        {
            throw new InvalidOperationException($"Inbox entry '{message.MessageId}' was not found.");
        }

        entry.Status = (int)IncomingMessageInboxStatus.Processed;
        entry.UpdatedAt = DateTimeOffset.UtcNow;
        entry.Error = null;
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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
            await set.SingleOrDefaultAsync(current => current.MessageId == message.MessageId, cancellationToken).ConfigureAwait(false);

        if (entry is null)
        {
            throw new InvalidOperationException($"Inbox entry '{message.MessageId}' was not found.");
        }

        entry.Status = (int)IncomingMessageInboxStatus.Failed;
        entry.UpdatedAt = DateTimeOffset.UtcNow;
        entry.Error = exception.Message;
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IncomingMessageInboxEntry?> GetAsync(string messageId, CancellationToken cancellationToken = default)
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

    private async Task<bool> HandleExistingEntryAsync(
        MessagingInboxEntryEntity existing,
        IncomingTransportMessage message,
        CancellationToken cancellationToken)
    {
        if ((IncomingMessageInboxStatus)existing.Status == IncomingMessageInboxStatus.Processed)
        {
            return false;
        }

        if ((IncomingMessageInboxStatus)existing.Status == IncomingMessageInboxStatus.Processing)
        {
            return await TryReclaimStaleProcessingEntryAsync(existing, message, cancellationToken).ConfigureAwait(false);
        }

        return await TryResumeFailedEntryAsync(message, cancellationToken).ConfigureAwait(false);
    }

    private async Task<bool> TryResumeFailedEntryAsync(
        IncomingTransportMessage message,
        CancellationToken cancellationToken)
    {
        var set = databaseContext.Set<MessagingInboxEntryEntity>();
        var entry = set.Local.SingleOrDefault(current => current.MessageId == message.MessageId) ??
            await set.SingleOrDefaultAsync(current => current.MessageId == message.MessageId, cancellationToken).ConfigureAwait(false);

        if (entry is null)
        {
            return false;
        }

        if ((IncomingMessageInboxStatus)entry.Status is IncomingMessageInboxStatus.Processing or IncomingMessageInboxStatus.Processed)
        {
            return false;
        }

        entry.Status = (int)IncomingMessageInboxStatus.Processing;
        entry.UpdatedAt = DateTimeOffset.UtcNow;
        entry.Error = null;
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private async Task<MessagingInboxEntryEntity?> GetCurrentEntryAsync(
        string messageId,
        CancellationToken cancellationToken)
    {
        return await databaseContext.Set<MessagingInboxEntryEntity>()
            .AsNoTracking()
            .SingleOrDefaultAsync(current => current.MessageId == messageId, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<bool> TryReclaimStaleProcessingEntryAsync(
        MessagingInboxEntryEntity entry,
        IncomingTransportMessage message,
        CancellationToken cancellationToken)
    {
        if (!IsProcessingLeaseStale(entry.UpdatedAt))
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        var rowsAffected = await databaseContext.Set<MessagingInboxEntryEntity>()
            .Where(current =>
                current.MessageId == message.MessageId &&
                current.Status == (int)IncomingMessageInboxStatus.Processing &&
                current.UpdatedAt == entry.UpdatedAt)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(static current => current.Destination, _ => message.Destination)
                    .SetProperty(static current => current.UpdatedAt, _ => now)
                    .SetProperty(static current => current.Error, _ => null),
                cancellationToken)
            .ConfigureAwait(false);

        return rowsAffected == 1;
    }

    private bool IsProcessingLeaseStale(DateTimeOffset updatedAt)
    {
        return DateTimeOffset.UtcNow - updatedAt >= configuration.IncomingProcessing.ProcessingLeaseTimeout;
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Database;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Database.Configurations;
using Raycynix.Extensions.Messaging.Database.Models;

namespace Raycynix.Extensions.Messaging.Database.Implementations;

/// <summary>
/// Removes expired inbox and outbox rows from the messaging persistence tables.
/// </summary>
public sealed class MessagingDatabaseCleanupProcessor(
    RaycynixDatabaseContext databaseContext,
    MessagingDatabasePersistenceConfiguration configuration,
    ILogger<MessagingDatabaseCleanupProcessor>? logger = null)
{
    /// <summary>
    /// Runs a single cleanup cycle for expired inbox and outbox rows.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The total number of deleted rows.</returns>
    public async Task<int> ProcessAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var deletedInbox = await CleanupInboxAsync(now, cancellationToken).ConfigureAwait(false);
        var deletedOutbox = await CleanupOutboxAsync(now, cancellationToken).ConfigureAwait(false);
        logger?.LogDebug(
            "Messaging database cleanup processed expired rows. DeletedInboxCount={DeletedInboxCount}, DeletedOutboxCount={DeletedOutboxCount}.",
            deletedInbox,
            deletedOutbox);

        return deletedInbox + deletedOutbox;
    }

    private async Task<int> CleanupInboxAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var processedStatus = (int)IncomingMessageInboxStatus.Processed;
        var failedStatus = (int)IncomingMessageInboxStatus.Failed;

        var candidates = await databaseContext.Set<MessagingInboxEntryEntity>()
            .AsNoTracking()
            .Where(entry => entry.Status == processedStatus || entry.Status == failedStatus)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var expiredIds = candidates
            .Where(entry => IsInboxExpired(entry, now))
            .OrderBy(entry => entry.UpdatedAt)
            .Take(configuration.CleanupBatchSize)
            .Select(entry => entry.MessageId)
            .ToArray();

        if (expiredIds.Length == 0)
        {
            return 0;
        }

        var entries = await databaseContext.Set<MessagingInboxEntryEntity>()
            .Where(entry => expiredIds.AsEnumerable().Contains(entry.MessageId))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        databaseContext.RemoveRange(entries);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entries.Length;
    }

    private async Task<int> CleanupOutboxAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        var dispatchedStatus = (int)MessageOutboxStatus.Dispatched;
        var failedStatus = (int)MessageOutboxStatus.Failed;

        var candidates = await databaseContext.Set<MessagingOutboxEntryEntity>()
            .AsNoTracking()
            .Where(entry => entry.Status == dispatchedStatus || entry.Status == failedStatus)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        var expiredIds = candidates
            .Where(entry => IsOutboxExpired(entry, now))
            .OrderBy(entry => entry.UpdatedAt)
            .Take(configuration.CleanupBatchSize)
            .Select(entry => entry.MessageId)
            .ToArray();

        if (expiredIds.Length == 0)
        {
            return 0;
        }

        var entries = await databaseContext.Set<MessagingOutboxEntryEntity>()
            .Where(entry => expiredIds.AsEnumerable().Contains(entry.MessageId))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        databaseContext.RemoveRange(entries);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entries.Length;
    }

    private bool IsInboxExpired(MessagingInboxEntryEntity entry, DateTimeOffset now)
    {
        var retention = (IncomingMessageInboxStatus)entry.Status switch
        {
            IncomingMessageInboxStatus.Processed => configuration.ProcessedInboxRetention,
            IncomingMessageInboxStatus.Failed => configuration.FailedInboxRetention,
            _ => TimeSpan.MaxValue
        };

        return now - entry.UpdatedAt >= retention;
    }

    private bool IsOutboxExpired(MessagingOutboxEntryEntity entry, DateTimeOffset now)
    {
        var retention = (MessageOutboxStatus)entry.Status switch
        {
            MessageOutboxStatus.Dispatched => configuration.DispatchedOutboxRetention,
            MessageOutboxStatus.Failed => configuration.FailedOutboxRetention,
            _ => TimeSpan.MaxValue
        };

        return now - entry.UpdatedAt >= retention;
    }
}
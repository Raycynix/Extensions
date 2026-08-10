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
        var processedCutoff = now - configuration.ProcessedInboxRetention;
        var failedCutoff = now - configuration.FailedInboxRetention;

        var expiredIds = await databaseContext.Set<MessagingInboxEntryEntity>()
            .AsNoTracking()
            .Where(entry =>
                (entry.Status == processedStatus && entry.UpdatedAt <= processedCutoff) ||
                (entry.Status == failedStatus && entry.UpdatedAt <= failedCutoff))
            .OrderBy(entry => entry.UpdatedAt)
            .Take(configuration.CleanupBatchSize)
            .Select(entry => entry.MessageId)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        if (expiredIds.Length == 0)
        {
            return 0;
        }

        var entries = await databaseContext.Set<MessagingInboxEntryEntity>()
            .Where(entry => expiredIds.Contains(entry.MessageId))
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
        var dispatchedCutoff = now - configuration.DispatchedOutboxRetention;
        var failedCutoff = now - configuration.FailedOutboxRetention;

        var expiredIds = await databaseContext.Set<MessagingOutboxEntryEntity>()
            .AsNoTracking()
            .Where(entry =>
                (entry.Status == dispatchedStatus && entry.UpdatedAt <= dispatchedCutoff) ||
                (entry.Status == failedStatus && entry.UpdatedAt <= failedCutoff))
            .OrderBy(entry => entry.UpdatedAt)
            .Take(configuration.CleanupBatchSize)
            .Select(entry => entry.MessageId)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        if (expiredIds.Length == 0)
        {
            return 0;
        }

        var entries = await databaseContext.Set<MessagingOutboxEntryEntity>()
            .Where(entry => expiredIds.Contains(entry.MessageId))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        databaseContext.RemoveRange(entries);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entries.Length;
    }

}

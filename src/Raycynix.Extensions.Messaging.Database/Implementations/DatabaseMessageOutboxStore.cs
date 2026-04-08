using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.Implementations;
using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Database.Models;

namespace Raycynix.Extensions.Messaging.Database.Implementations;

/// <summary>
/// Persists outbox state for outgoing messages in the configured database.
/// </summary>
internal sealed class DatabaseMessageOutboxStore(
    DatabaseContext databaseContext) : ITransactionalMessageOutboxStore
{
    private static readonly HashSet<Type> MessagingEntityTypes =
    [
        typeof(MessagingOutboxEntryEntity),
        typeof(MessagingInboxEntryEntity)
    ];

    /// <inheritdoc />
    public bool ShouldDeferToAmbientUnitOfWork => databaseContext.Database.CurrentTransaction is not null || HasExternalPendingChanges();

    /// <inheritdoc />
    public async Task EnqueueAsync(SerializedMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        await EnqueueCoreAsync(message, cancellationToken).ConfigureAwait(false);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task EnqueueDeferredAsync(SerializedMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        return EnqueueCoreAsync(message, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> TryBeginDispatchAsync(
        string messageId,
        DateTimeOffset leaseUntil,
        CancellationToken cancellationToken = default)
    {
        return await TryBeginDispatchCoreAsync(messageId, leaseUntil, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<bool> TryBeginDispatchDeferredAsync(
        string messageId,
        DateTimeOffset leaseUntil,
        CancellationToken cancellationToken = default)
    {
        return TryBeginDispatchCoreAsync(messageId, leaseUntil, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<MessageOutboxEntry>> GetAvailableAsync(
        DateTimeOffset asOf,
        int maxCount,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxCount);

        var pendingStatus = (int)MessageOutboxStatus.Pending;
        var dispatchingStatus = (int)MessageOutboxStatus.Dispatching;
        var failedStatus = (int)MessageOutboxStatus.Failed;
        var entities = await databaseContext.Set<MessagingOutboxEntryEntity>()
            .AsNoTracking()
            .Where(entry =>
                entry.Status == pendingStatus ||
                entry.Status == failedStatus ||
                entry.Status == dispatchingStatus)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return entities
            .Where(entry => entry.NextAttemptAt <= asOf)
            .OrderBy(entry => entry.NextAttemptAt)
            .ThenBy(entry => entry.CreatedAt)
            .Take(maxCount)
            .Select(Map)
            .ToArray();
    }

    /// <inheritdoc />
    public async Task MarkDispatchedAsync(string messageId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);

        await MarkDispatchedCoreAsync(messageId, cancellationToken).ConfigureAwait(false);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task MarkDispatchedDeferredAsync(string messageId, CancellationToken cancellationToken = default)
    {
        return MarkDispatchedCoreAsync(messageId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task MarkFailedAsync(
        string messageId,
        Exception exception,
        DateTimeOffset nextAttemptAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);
        ArgumentNullException.ThrowIfNull(exception);

        await MarkFailedCoreAsync(messageId, exception, nextAttemptAt, cancellationToken).ConfigureAwait(false);
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task MarkFailedDeferredAsync(
        string messageId,
        Exception exception,
        DateTimeOffset nextAttemptAt,
        CancellationToken cancellationToken = default)
    {
        return MarkFailedCoreAsync(messageId, exception, nextAttemptAt, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<MessageOutboxEntry?> GetAsync(string messageId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);

        var entry = databaseContext.ChangeTracker.Entries<MessagingOutboxEntryEntity>()
            .Where(current => current.Entity.MessageId == messageId)
            .Select(current => current.Entity)
            .SingleOrDefault() ?? await databaseContext.Set<MessagingOutboxEntryEntity>()
            .AsNoTracking()
            .SingleOrDefaultAsync(current => current.MessageId == messageId, cancellationToken)
            .ConfigureAwait(false);

        return entry is null ? null : Map(entry);
    }

    /// <inheritdoc />
    public Task FlushAsync(CancellationToken cancellationToken = default)
    {
        return databaseContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Adds a new outbox entry or resets an existing one back to the pending state before it is flushed.
    /// </summary>
    /// <param name="message">The serialized message to persist.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task EnqueueCoreAsync(SerializedMessage message, CancellationToken cancellationToken)
    {
        var set = databaseContext.Set<MessagingOutboxEntryEntity>();
        var existing = set.Local.SingleOrDefault(entry => entry.MessageId == message.MessageId) ??
            await set.SingleOrDefaultAsync(entry => entry.MessageId == message.MessageId, cancellationToken).ConfigureAwait(false);
        var now = DateTimeOffset.UtcNow;

        if (existing is null)
        {
            set.Add(new MessagingOutboxEntryEntity
            {
                MessageId = message.MessageId,
                Destination = message.Destination,
                Payload = message.Payload,
                Format = (int)message.Format,
                ContentType = message.ContentType,
                CorrelationId = message.CorrelationId,
                CausationId = message.CausationId,
                CreatedAt = message.CreatedAt,
                Headers = JsonSerializer.Serialize(message.Headers),
                Status = (int)MessageOutboxStatus.Pending,
                UpdatedAt = now,
                AttemptCount = 0,
                NextAttemptAt = now
            });
            return;
        }

        existing.Destination = message.Destination;
        existing.Payload = message.Payload;
        existing.Format = (int)message.Format;
        existing.ContentType = message.ContentType;
        existing.CorrelationId = message.CorrelationId;
        existing.CausationId = message.CausationId;
        existing.CreatedAt = message.CreatedAt;
        existing.Headers = JsonSerializer.Serialize(message.Headers);
        existing.Status = (int)MessageOutboxStatus.Pending;
        existing.UpdatedAt = now;
        existing.NextAttemptAt = now;
        existing.Error = null;
    }

    /// <summary>
    /// Attempts to acquire an optimistic dispatch lease for the specified outbox entry.
    /// </summary>
    /// <param name="messageId">The identifier of the outbox entry to lease.</param>
    /// <param name="leaseUntil">The timestamp until which the lease remains valid.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// <see langword="true"/> when the current worker wins the optimistic concurrency race; otherwise, <see langword="false"/>.
    /// </returns>
    private async Task<bool> TryBeginDispatchCoreAsync(
        string messageId,
        DateTimeOffset leaseUntil,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);

        var set = databaseContext.Set<MessagingOutboxEntryEntity>();
        var entry = set.Local.SingleOrDefault(current => current.MessageId == messageId) ??
            await set
                .SingleOrDefaultAsync(current => current.MessageId == messageId, cancellationToken)
                .ConfigureAwait(false);

        if (entry is null)
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        var canLease = (MessageOutboxStatus)entry.Status switch
        {
            MessageOutboxStatus.Pending => entry.NextAttemptAt <= now,
            MessageOutboxStatus.Failed => entry.NextAttemptAt <= now,
            MessageOutboxStatus.Dispatching => entry.NextAttemptAt <= now,
            _ => false
        };

        if (!canLease)
        {
            return false;
        }

        var previousStatus = (MessageOutboxStatus)entry.Status;
        entry.Status = (int)MessageOutboxStatus.Dispatching;
        entry.UpdatedAt = now;
        entry.NextAttemptAt = leaseUntil;
        if (previousStatus != MessageOutboxStatus.Failed)
        {
            entry.Error = null;
        }

        try
        {
            await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            databaseContext.Entry(entry).State = EntityState.Unchanged;
            return false;
        }
    }

    /// <summary>
    /// Marks an outbox entry as dispatched inside the current unit of work.
    /// </summary>
    /// <param name="messageId">The identifier of the outbox entry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task MarkDispatchedCoreAsync(string messageId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);

        var set = databaseContext.Set<MessagingOutboxEntryEntity>();
        var entry = set.Local.SingleOrDefault(current => current.MessageId == messageId) ??
            await set.SingleOrDefaultAsync(current => current.MessageId == messageId, cancellationToken)
            .ConfigureAwait(false);

        if (entry is null)
        {
            throw new InvalidOperationException($"Outbox entry '{messageId}' was not found.");
        }

        entry.Status = (int)MessageOutboxStatus.Dispatched;
        entry.UpdatedAt = DateTimeOffset.UtcNow;
        entry.AttemptCount++;
        entry.NextAttemptAt = DateTimeOffset.MaxValue;
        entry.Error = null;
    }

    /// <summary>
    /// Marks an outbox entry as failed inside the current unit of work.
    /// </summary>
    /// <param name="messageId">The identifier of the outbox entry.</param>
    /// <param name="exception">The exception that caused the dispatch failure.</param>
    /// <param name="nextAttemptAt">The next timestamp when the message may be retried.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task MarkFailedCoreAsync(
        string messageId,
        Exception exception,
        DateTimeOffset nextAttemptAt,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);
        ArgumentNullException.ThrowIfNull(exception);

        var set = databaseContext.Set<MessagingOutboxEntryEntity>();
        var entry = set.Local.SingleOrDefault(current => current.MessageId == messageId) ??
            await set.SingleOrDefaultAsync(current => current.MessageId == messageId, cancellationToken)
            .ConfigureAwait(false);

        if (entry is null)
        {
            throw new InvalidOperationException($"Outbox entry '{messageId}' was not found.");
        }

        entry.Status = (int)MessageOutboxStatus.Failed;
        entry.UpdatedAt = DateTimeOffset.UtcNow;
        entry.AttemptCount++;
        entry.NextAttemptAt = nextAttemptAt;
        entry.Error = exception.Message;
    }

    /// <summary>
    /// Determines whether the current change tracker contains pending changes outside the messaging entities.
    /// </summary>
    /// <returns><see langword="true"/> when the caller should defer flushing to an ambient unit of work.</returns>
    private bool HasExternalPendingChanges()
    {
        return databaseContext.ChangeTracker.Entries()
            .Where(static entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Any(entry => !MessagingEntityTypes.Contains(entry.Entity.GetType()));
    }

    /// <summary>
    /// Maps a tracked outbox entity to the public outbox entry model.
    /// </summary>
    /// <param name="entry">The database entity to map.</param>
    /// <returns>The public outbox entry representation.</returns>
    private static MessageOutboxEntry Map(MessagingOutboxEntryEntity entry)
    {
        return new MessageOutboxEntry
        {
            Message = new SerializedMessage
            {
                MessageId = entry.MessageId,
                Destination = entry.Destination,
                Payload = entry.Payload,
                Format = (MessageFormat)entry.Format,
                ContentType = entry.ContentType,
                CorrelationId = entry.CorrelationId,
                CausationId = entry.CausationId,
                CreatedAt = entry.CreatedAt,
                Headers = JsonSerializer.Deserialize<Dictionary<string, string>>(entry.Headers) ??
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            },
            Status = (MessageOutboxStatus)entry.Status,
            CreatedAt = entry.CreatedAt,
            UpdatedAt = entry.UpdatedAt,
            AttemptCount = entry.AttemptCount,
            NextAttemptAt = entry.NextAttemptAt,
            Error = entry.Error
        };
    }
}

using System.Collections.Concurrent;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;

namespace Raycynix.Extensions.Messaging.Implementations;

/// <summary>
/// Provides an in-memory outbox store for outgoing message dispatch and recovery.
/// </summary>
public sealed class InMemoryMessageOutboxStore : IMessageOutboxStore
{
    private readonly ConcurrentDictionary<string, MessageOutboxEntry> _entries = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public Task EnqueueAsync(SerializedMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();

        var now = DateTimeOffset.UtcNow;
        _entries.AddOrUpdate(
            message.MessageId,
            _ => CreateEntry(message, MessageOutboxStatus.Pending, now, now, 0, now),
            (_, existingEntry) => existingEntry with
            {
                Message = message,
                Status = MessageOutboxStatus.Pending,
                UpdatedAt = now,
                NextAttemptAt = now,
                Error = null
            });

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<MessageOutboxEntry>> GetAvailableAsync(
        DateTimeOffset asOf,
        int maxCount,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxCount);
        cancellationToken.ThrowIfCancellationRequested();

        var entries = _entries.Values
            .Where(entry => entry.Status is MessageOutboxStatus.Pending or MessageOutboxStatus.Failed)
            .Where(entry => entry.NextAttemptAt <= asOf)
            .OrderBy(entry => entry.NextAttemptAt)
            .ThenBy(entry => entry.CreatedAt)
            .Take(maxCount)
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<MessageOutboxEntry>>(entries);
    }

    /// <inheritdoc />
    public Task MarkDispatchedAsync(string messageId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);
        cancellationToken.ThrowIfCancellationRequested();

        UpdateEntry(
            messageId,
            existingEntry => existingEntry with
            {
                Status = MessageOutboxStatus.Dispatched,
                UpdatedAt = DateTimeOffset.UtcNow,
                AttemptCount = existingEntry.AttemptCount + 1,
                NextAttemptAt = DateTimeOffset.MaxValue,
                Error = null
            });

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task MarkFailedAsync(
        string messageId,
        Exception exception,
        DateTimeOffset nextAttemptAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);
        ArgumentNullException.ThrowIfNull(exception);
        cancellationToken.ThrowIfCancellationRequested();

        UpdateEntry(
            messageId,
            existingEntry => existingEntry with
            {
                Status = MessageOutboxStatus.Failed,
                UpdatedAt = DateTimeOffset.UtcNow,
                AttemptCount = existingEntry.AttemptCount + 1,
                NextAttemptAt = nextAttemptAt,
                Error = exception.Message
            });

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<MessageOutboxEntry?> GetAsync(string messageId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);
        cancellationToken.ThrowIfCancellationRequested();

        _entries.TryGetValue(messageId, out var entry);
        return Task.FromResult(entry);
    }

    private static MessageOutboxEntry CreateEntry(
        SerializedMessage message,
        MessageOutboxStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        int attemptCount,
        DateTimeOffset nextAttemptAt,
        string? error = null)
    {
        return new MessageOutboxEntry
        {
            Message = message,
            Status = status,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            AttemptCount = attemptCount,
            NextAttemptAt = nextAttemptAt,
            Error = error
        };
    }

    private void UpdateEntry(string messageId, Func<MessageOutboxEntry, MessageOutboxEntry> update)
    {
        while (true)
        {
            if (!_entries.TryGetValue(messageId, out var existingEntry))
            {
                throw new InvalidOperationException($"Outbox entry '{messageId}' was not found.");
            }

            var updatedEntry = update(existingEntry);
            if (_entries.TryUpdate(messageId, updatedEntry, existingEntry))
            {
                return;
            }
        }
    }
}

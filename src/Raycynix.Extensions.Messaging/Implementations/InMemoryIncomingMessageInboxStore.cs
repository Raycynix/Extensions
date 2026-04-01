using System.Collections.Concurrent;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;

namespace Raycynix.Extensions.Messaging.Implementations;

/// <summary>
/// Provides an in-memory inbox store for deduplicating and tracking incoming message processing.
/// </summary>
public sealed class InMemoryIncomingMessageInboxStore : IIncomingMessageInboxStore
{
    private readonly ConcurrentDictionary<string, IncomingMessageInboxEntry> _entries = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public Task<bool> TryBeginProcessingAsync(IncomingTransportMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_entries.TryGetValue(message.MessageId, out var existingEntry))
            {
                var createdEntry = CreateEntry(message, IncomingMessageInboxStatus.Processing);
                if (_entries.TryAdd(message.MessageId, createdEntry))
                {
                    return Task.FromResult(true);
                }

                continue;
            }

            if (existingEntry.Status is IncomingMessageInboxStatus.Processed or IncomingMessageInboxStatus.Processing)
            {
                return Task.FromResult(false);
            }

            var processingEntry = CreateEntry(message, IncomingMessageInboxStatus.Processing);
            if (_entries.TryUpdate(message.MessageId, processingEntry, existingEntry))
            {
                return Task.FromResult(true);
            }
        }
    }

    /// <inheritdoc />
    public Task MarkProcessedAsync(IncomingTransportMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();

        _entries.AddOrUpdate(
            message.MessageId,
            _ => CreateEntry(message, IncomingMessageInboxStatus.Processed),
            (_, existingEntry) => CreateEntry(message, IncomingMessageInboxStatus.Processed, existingEntry.Error));

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task MarkFailedAsync(
        IncomingTransportMessage message,
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(exception);
        cancellationToken.ThrowIfCancellationRequested();

        _entries.AddOrUpdate(
            message.MessageId,
            _ => CreateEntry(message, IncomingMessageInboxStatus.Failed, exception.Message),
            (_, _) => CreateEntry(message, IncomingMessageInboxStatus.Failed, exception.Message));

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IncomingMessageInboxEntry?> GetAsync(string messageId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);
        cancellationToken.ThrowIfCancellationRequested();

        _entries.TryGetValue(messageId, out var entry);
        return Task.FromResult(entry);
    }

    private static IncomingMessageInboxEntry CreateEntry(
        IncomingTransportMessage message,
        IncomingMessageInboxStatus status,
        string? error = null)
    {
        return new IncomingMessageInboxEntry
        {
            MessageId = message.MessageId,
            Destination = message.Destination,
            Status = status,
            UpdatedAt = DateTimeOffset.UtcNow,
            Error = error
        };
    }
}

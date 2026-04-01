using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
    IServiceScopeFactory serviceScopeFactory) : IMessageOutboxStore
{
    /// <inheritdoc />
    public async Task EnqueueAsync(SerializedMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var set = databaseContext.Set<MessagingOutboxEntryEntity>();
        var existing = await set.SingleOrDefaultAsync(entry => entry.MessageId == message.MessageId, cancellationToken).ConfigureAwait(false);
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
        }
        else
        {
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

        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<MessageOutboxEntry>> GetAvailableAsync(
        DateTimeOffset asOf,
        int maxCount,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxCount);

        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var pendingStatus = (int)MessageOutboxStatus.Pending;
        var failedStatus = (int)MessageOutboxStatus.Failed;
        var entities = await databaseContext.Set<MessagingOutboxEntryEntity>()
            .AsNoTracking()
            .Where(entry => entry.Status == pendingStatus || entry.Status == failedStatus)
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

        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var entry = await databaseContext.Set<MessagingOutboxEntryEntity>()
            .SingleOrDefaultAsync(current => current.MessageId == messageId, cancellationToken)
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
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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

        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var entry = await databaseContext.Set<MessagingOutboxEntryEntity>()
            .SingleOrDefaultAsync(current => current.MessageId == messageId, cancellationToken)
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
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<MessageOutboxEntry?> GetAsync(string messageId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageId);

        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var entry = await databaseContext.Set<MessagingOutboxEntryEntity>()
            .AsNoTracking()
            .SingleOrDefaultAsync(current => current.MessageId == messageId, cancellationToken)
            .ConfigureAwait(false);

        return entry is null ? null : Map(entry);
    }

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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Implementations;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Database.Models;

namespace Raycynix.Extensions.Messaging.Database.Implementations;

/// <summary>
/// Persists inbox state for incoming messages in the configured database.
/// </summary>
internal sealed class DatabaseIncomingMessageInboxStore(
    IServiceScopeFactory serviceScopeFactory) : IIncomingMessageInboxStore
{
    /// <inheritdoc />
    public async Task<bool> TryBeginProcessingAsync(IncomingTransportMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var existing = await databaseContext.Set<MessagingInboxEntryEntity>()
            .SingleOrDefaultAsync(entry => entry.MessageId == message.MessageId, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            databaseContext.Set<MessagingInboxEntryEntity>().Add(new MessagingInboxEntryEntity
            {
                MessageId = message.MessageId,
                Destination = message.Destination,
                Status = (int)IncomingMessageInboxStatus.Processing,
                UpdatedAt = DateTimeOffset.UtcNow
            });
            await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        if ((IncomingMessageInboxStatus)existing.Status is IncomingMessageInboxStatus.Processing or IncomingMessageInboxStatus.Processed)
        {
            return false;
        }

        existing.Status = (int)IncomingMessageInboxStatus.Processing;
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        existing.Error = null;
        await databaseContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public async Task MarkProcessedAsync(IncomingTransportMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var entry = await databaseContext.Set<MessagingInboxEntryEntity>()
            .SingleOrDefaultAsync(current => current.MessageId == message.MessageId, cancellationToken)
            .ConfigureAwait(false);

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

        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var entry = await databaseContext.Set<MessagingInboxEntryEntity>()
            .SingleOrDefaultAsync(current => current.MessageId == message.MessageId, cancellationToken)
            .ConfigureAwait(false);

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

        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var entry = await databaseContext.Set<MessagingInboxEntryEntity>()
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
}

using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Configurations;

namespace Raycynix.Extensions.Messaging.Implementations;

/// <summary>
/// Republishes pending and failed outbox messages through the configured transport.
/// </summary>
public sealed class MessageOutboxRecoveryProcessor(
    IEnumerable<ITransportMessagePublisher> transportPublishers,
    IMessageOutboxStore outboxStore,
    MessagingConfiguration configuration,
    ILogger<MessageOutboxRecoveryProcessor>? logger = null)
{
    /// <summary>
    /// Processes a single outbox recovery cycle.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of messages dispatched successfully.</returns>
    public async Task<int> ProcessAvailableAsync(CancellationToken cancellationToken = default)
    {
        if (!configuration.Outbox.Enabled)
        {
            logger?.LogDebug("Outbox recovery skipped because outbox is disabled.");
            return 0;
        }

        var transportPublisher = ResolveTransportPublisher(transportPublishers);
        var entries = await outboxStore.GetAvailableAsync(
                DateTimeOffset.UtcNow,
                configuration.Outbox.RecoveryBatchSize,
                cancellationToken)
            .ConfigureAwait(false);

        logger?.LogDebug("Outbox recovery loaded {MessageCount} available message(s).", entries.Count);

        var successCount = 0;
        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var leased = await outboxStore.TryBeginDispatchAsync(
                    entry.Message.MessageId,
                    DateTimeOffset.UtcNow.Add(configuration.Outbox.DispatchLeaseTimeout),
                    cancellationToken)
                .ConfigureAwait(false);
            if (!leased)
            {
                logger?.LogDebug("Outbox recovery could not acquire dispatch lease. Destination={Destination}.",
                    entry.Message.Destination);
                continue;
            }

            try
            {
                await transportPublisher.PublishAsync(entry.Message, cancellationToken).ConfigureAwait(false);
                await outboxStore.MarkDispatchedAsync(entry.Message.MessageId, cancellationToken).ConfigureAwait(false);
                successCount++;
                logger?.LogDebug("Outbox recovery published message. Destination={Destination}.",
                    entry.Message.Destination);
            }
            catch (Exception exception)
            {
                await outboxStore.MarkFailedAsync(
                        entry.Message.MessageId,
                        exception,
                        DateTimeOffset.UtcNow.Add(configuration.Outbox.RetryDelay),
                        cancellationToken)
                    .ConfigureAwait(false);
                logger?.LogWarning(exception, "Outbox recovery publish failed. Destination={Destination}.",
                    entry.Message.Destination);
            }
        }

        logger?.LogDebug("Outbox recovery completed. SuccessCount={SuccessCount}, AvailableCount={AvailableCount}.",
            successCount, entries.Count);
        return successCount;
    }

    private static ITransportMessagePublisher ResolveTransportPublisher(
        IEnumerable<ITransportMessagePublisher> transportPublishers)
    {
        var publishers = transportPublishers.ToArray();
        return publishers.Length switch
        {
            0 => throw new InvalidOperationException(
                "No message transport publisher is configured. Register Raycynix.Extensions.Messaging.Kafka or Raycynix.Extensions.Messaging.RabbitMQ."),
            1 => publishers[0],
            _ => throw new InvalidOperationException(
                "Multiple message transport publishers are configured. Register only one transport publisher.")
        };
    }
}
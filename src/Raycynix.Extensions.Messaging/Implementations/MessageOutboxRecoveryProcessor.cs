using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Configurations;

namespace Raycynix.Extensions.Messaging.Implementations;

/// <summary>
/// Republishes pending and failed outbox messages through the configured transport.
/// </summary>
public sealed class MessageOutboxRecoveryProcessor(
    IEnumerable<ITransportMessagePublisher> transportPublishers,
    IMessageOutboxStore outboxStore,
    MessagingConfiguration configuration)
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
            return 0;
        }

        var transportPublisher = ResolveTransportPublisher(transportPublishers);
        var entries = await outboxStore.GetAvailableAsync(
                DateTimeOffset.UtcNow,
                configuration.Outbox.RecoveryBatchSize,
                cancellationToken)
            .ConfigureAwait(false);

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
                continue;
            }

            try
            {
                await transportPublisher.PublishAsync(entry.Message, cancellationToken).ConfigureAwait(false);
                await outboxStore.MarkDispatchedAsync(entry.Message.MessageId, cancellationToken).ConfigureAwait(false);
                successCount++;
            }
            catch (Exception exception)
            {
                await outboxStore.MarkFailedAsync(
                        entry.Message.MessageId,
                        exception,
                        DateTimeOffset.UtcNow.Add(configuration.Outbox.RetryDelay),
                        cancellationToken)
                    .ConfigureAwait(false);
            }
        }

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

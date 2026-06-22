using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Configurations;

namespace Raycynix.Extensions.Messaging.Internal;

internal sealed class MessagePublisher(
    IMessageSerializer serializer,
    IEnumerable<ITransportMessagePublisher> transportPublishers,
    IMessageOutboxStore outboxStore,
    MessagingConfiguration configuration,
    ILogger<MessagePublisher>? logger = null) : IMessagePublisher
{
    /// <inheritdoc />
    public async ValueTask PublishAsync<TMessage>(MessageEnvelope<TMessage> envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var transportPublisher = ResolveTransportPublisher(transportPublishers);
        var serialized = serializer.Serialize(envelope);

        logger?.LogDebug(
            "Publishing message. MessageType={MessageType}, Destination={Destination}, Format={Format}, OutboxEnabled={OutboxEnabled}, HeaderCount={HeaderCount}.",
            typeof(TMessage).FullName,
            envelope.Destination,
            envelope.Format,
            configuration.Outbox.Enabled,
            envelope.Headers.Count);

        if (!configuration.Outbox.Enabled)
        {
            await transportPublisher.PublishAsync(serialized, cancellationToken).ConfigureAwait(false);
            logger?.LogDebug(
                "Published message directly without outbox. Destination={Destination}, Format={Format}.",
                serialized.Destination,
                serialized.Format);
            return;
        }

        if (outboxStore is ITransactionalMessageOutboxStore transactionalOutboxStore)
        {
            await transactionalOutboxStore.EnqueueDeferredAsync(serialized, cancellationToken).ConfigureAwait(false);
            logger?.LogDebug(
                "Enqueued message into transactional outbox. Destination={Destination}, DeferredToAmbientUnitOfWork={DeferredToAmbientUnitOfWork}.",
                serialized.Destination,
                transactionalOutboxStore.ShouldDeferToAmbientUnitOfWork);

            if (transactionalOutboxStore.ShouldDeferToAmbientUnitOfWork)
            {
                return;
            }

            await transactionalOutboxStore.FlushAsync(cancellationToken).ConfigureAwait(false);

            if (!configuration.Outbox.AutoDispatchOnPublish)
            {
                logger?.LogDebug("Outbox auto-dispatch is disabled for published message. Destination={Destination}.",
                    serialized.Destination);
                return;
            }

            var leased = await transactionalOutboxStore.TryBeginDispatchDeferredAsync(
                    serialized.MessageId,
                    DateTimeOffset.UtcNow.Add(configuration.Outbox.DispatchLeaseTimeout),
                    cancellationToken)
                .ConfigureAwait(false);

            if (!leased)
            {
                await transactionalOutboxStore.FlushAsync(cancellationToken).ConfigureAwait(false);
                logger?.LogDebug("Transactional outbox dispatch lease was not acquired. Destination={Destination}.",
                    serialized.Destination);
                return;
            }

            await transactionalOutboxStore.FlushAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                await transportPublisher.PublishAsync(serialized, cancellationToken).ConfigureAwait(false);
                await transactionalOutboxStore.MarkDispatchedDeferredAsync(serialized.MessageId, cancellationToken)
                    .ConfigureAwait(false);
                await transactionalOutboxStore.FlushAsync(cancellationToken).ConfigureAwait(false);
                logger?.LogDebug("Published transactional outbox message. Destination={Destination}.",
                    serialized.Destination);
            }
            catch (Exception exception)
            {
                await transactionalOutboxStore.MarkFailedDeferredAsync(
                        serialized.MessageId,
                        exception,
                        DateTimeOffset.UtcNow.Add(configuration.Outbox.RetryDelay),
                        cancellationToken)
                    .ConfigureAwait(false);
                await transactionalOutboxStore.FlushAsync(cancellationToken).ConfigureAwait(false);
                logger?.LogWarning(exception, "Transactional outbox message publish failed. Destination={Destination}.",
                    serialized.Destination);
                throw;
            }

            return;
        }

        await outboxStore.EnqueueAsync(serialized, cancellationToken).ConfigureAwait(false);
        logger?.LogDebug("Enqueued message into outbox. Destination={Destination}.", serialized.Destination);

        if (!configuration.Outbox.AutoDispatchOnPublish)
        {
            logger?.LogDebug("Outbox auto-dispatch is disabled for published message. Destination={Destination}.",
                serialized.Destination);
            return;
        }

        var acquired = await outboxStore.TryBeginDispatchAsync(
                serialized.MessageId,
                DateTimeOffset.UtcNow.Add(configuration.Outbox.DispatchLeaseTimeout),
                cancellationToken)
            .ConfigureAwait(false);

        if (!acquired)
        {
            logger?.LogDebug("Outbox dispatch lease was not acquired. Destination={Destination}.",
                serialized.Destination);
            return;
        }

        try
        {
            await transportPublisher.PublishAsync(serialized, cancellationToken).ConfigureAwait(false);
            await outboxStore.MarkDispatchedAsync(serialized.MessageId, cancellationToken).ConfigureAwait(false);
            logger?.LogDebug("Published outbox message. Destination={Destination}.", serialized.Destination);
        }
        catch (Exception exception)
        {
            await outboxStore.MarkFailedAsync(
                    serialized.MessageId,
                    exception,
                    DateTimeOffset.UtcNow.Add(configuration.Outbox.RetryDelay),
                    cancellationToken)
                .ConfigureAwait(false);
            logger?.LogWarning(exception, "Outbox message publish failed. Destination={Destination}.",
                serialized.Destination);
            throw;
        }
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
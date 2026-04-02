using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Configurations;

namespace Raycynix.Extensions.Messaging.Internal;

internal sealed class MessagePublisher(
    IMessageSerializer serializer,
    IEnumerable<ITransportMessagePublisher> transportPublishers,
    IMessageOutboxStore outboxStore,
    MessagingConfiguration configuration) : IMessagePublisher
{
    /// <inheritdoc />
    public async ValueTask PublishAsync<TMessage>(MessageEnvelope<TMessage> envelope, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var transportPublisher = ResolveTransportPublisher(transportPublishers);
        var serialized = serializer.Serialize(envelope);

        if (!configuration.Outbox.Enabled)
        {
            await transportPublisher.PublishAsync(serialized, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (outboxStore is ITransactionalMessageOutboxStore transactionalOutboxStore)
        {
            await transactionalOutboxStore.EnqueueDeferredAsync(serialized, cancellationToken).ConfigureAwait(false);

            if (transactionalOutboxStore.ShouldDeferToAmbientUnitOfWork)
            {
                return;
            }

            await transactionalOutboxStore.FlushAsync(cancellationToken).ConfigureAwait(false);

            if (!configuration.Outbox.AutoDispatchOnPublish)
            {
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
                return;
            }

            await transactionalOutboxStore.FlushAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                await transportPublisher.PublishAsync(serialized, cancellationToken).ConfigureAwait(false);
                await transactionalOutboxStore.MarkDispatchedDeferredAsync(serialized.MessageId, cancellationToken).ConfigureAwait(false);
                await transactionalOutboxStore.FlushAsync(cancellationToken).ConfigureAwait(false);
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
                throw;
            }

            return;
        }

        await outboxStore.EnqueueAsync(serialized, cancellationToken).ConfigureAwait(false);

        if (!configuration.Outbox.AutoDispatchOnPublish)
        {
            return;
        }

        var acquired = await outboxStore.TryBeginDispatchAsync(
                serialized.MessageId,
                DateTimeOffset.UtcNow.Add(configuration.Outbox.DispatchLeaseTimeout),
                cancellationToken)
            .ConfigureAwait(false);

        if (!acquired)
        {
            return;
        }

        try
        {
            await transportPublisher.PublishAsync(serialized, cancellationToken).ConfigureAwait(false);
            await outboxStore.MarkDispatchedAsync(serialized.MessageId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            await outboxStore.MarkFailedAsync(
                    serialized.MessageId,
                    exception,
                    DateTimeOffset.UtcNow.Add(configuration.Outbox.RetryDelay),
                    cancellationToken)
                .ConfigureAwait(false);
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

using RabbitMQ.Client;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Implementation;

namespace Raycynix.Extensions.Messaging.RabbitMQ.Internal;

/// <summary>
/// Publishes Raycynix message envelopes to RabbitMQ exchanges.
/// </summary>
internal sealed class RabbitMqMessagePublisher(
    IMessageSerializer serializer,
    RabbitMqConnectionAccessor connectionAccessor,
    Configurations.RabbitMqMessagingConfiguration configuration,
    MessageObservability observability) : IMessagePublisher
{
    /// <inheritdoc />
    public async ValueTask PublishAsync<TMessage>(
        MessageEnvelope<TMessage> envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var format = envelope.Format.ToString().ToLowerInvariant();
        using var observation = observability.BeginPublish(format, envelope.Destination);

        try
        {
            var serialized = serializer.Serialize(envelope);
            await using var channel = await connectionAccessor.CreateChannelAsync(cancellationToken).ConfigureAwait(false);

            var properties = new BasicProperties
            {
                MessageId = serialized.MessageId,
                CorrelationId = serialized.CorrelationId,
                ContentType = serialized.ContentType,
                Timestamp = new AmqpTimestamp(serialized.CreatedAt.ToUnixTimeSeconds()),
                Headers = serialized.Headers.ToDictionary(
                    pair => pair.Key, object? (pair) => System.Text.Encoding.UTF8.GetBytes(pair.Value))
            };

            if (!string.IsNullOrWhiteSpace(serialized.CausationId))
            {
                properties.Headers["causation-id"] = System.Text.Encoding.UTF8.GetBytes(serialized.CausationId);
            }

            await channel.BasicPublishAsync(
                    exchange: configuration.Exchange.Name,
                    routingKey: serialized.Destination,
                    mandatory: false,
                    basicProperties: properties,
                    body: serialized.Payload,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            observability.RecordPublishSuccess(format, envelope.Destination);
        }
        catch
        {
            observability.RecordPublishFailure(format, envelope.Destination);
            throw;
        }
    }
}

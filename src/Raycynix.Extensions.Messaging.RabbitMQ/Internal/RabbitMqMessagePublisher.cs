using RabbitMQ.Client;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;

namespace Raycynix.Extensions.Messaging.RabbitMQ.Internal;

/// <summary>
/// Publishes Raycynix message envelopes to RabbitMQ exchanges.
/// </summary>
internal sealed class RabbitMqMessagePublisher(
    IMessageSerializer serializer,
    RabbitMqConnectionAccessor connectionAccessor,
    Configurations.RabbitMqMessagingConfiguration configuration) : IMessagePublisher
{
    /// <inheritdoc />
    public async ValueTask PublishAsync<TMessage>(
        MessageEnvelope<TMessage> envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var serialized = serializer.Serialize(envelope);
        await using var channel = await connectionAccessor.CreateChannelAsync(cancellationToken).ConfigureAwait(false);

        var properties = new BasicProperties
        {
            MessageId = serialized.MessageId,
            CorrelationId = serialized.CorrelationId,
            ContentType = serialized.ContentType,
            Timestamp = new AmqpTimestamp(serialized.CreatedAt.ToUnixTimeSeconds()),
            Headers = serialized.Headers.ToDictionary(
                pair => pair.Key,
                pair => (object?)System.Text.Encoding.UTF8.GetBytes(pair.Value))
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
    }
}

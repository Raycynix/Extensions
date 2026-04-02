using RabbitMQ.Client;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Implementations;

namespace Raycynix.Extensions.Messaging.RabbitMQ.Internal;

/// <summary>
/// Publishes Raycynix message envelopes to RabbitMQ exchanges.
/// </summary>
internal sealed class RabbitMqMessagePublisher(
    RabbitMqConnectionAccessor connectionAccessor,
    Configurations.RabbitMqMessagingConfiguration configuration,
    MessageObservability observability) : ITransportMessagePublisher
{
    /// <inheritdoc />
    public async ValueTask PublishAsync(SerializedMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var format = message.Format.ToString().ToLowerInvariant();
        using var observation = observability.BeginPublish(format, message.Destination);

        try
        {
            await using var channel = await connectionAccessor.CreateChannelAsync(cancellationToken).ConfigureAwait(false);

            var properties = new BasicProperties
            {
                MessageId = message.MessageId,
                CorrelationId = message.CorrelationId,
                ContentType = message.ContentType,
                Timestamp = new AmqpTimestamp(message.CreatedAt.ToUnixTimeSeconds()),
                Headers = message.Headers.ToDictionary(
                    pair => pair.Key, object? (pair) => System.Text.Encoding.UTF8.GetBytes(pair.Value))
            };

            if (!string.IsNullOrWhiteSpace(message.CausationId))
            {
                properties.Headers["causation-id"] = System.Text.Encoding.UTF8.GetBytes(message.CausationId);
            }

            await channel.BasicPublishAsync(
                    exchange: configuration.Exchange.Name,
                    routingKey: message.Destination,
                    mandatory: false,
                    basicProperties: properties,
                    body: message.Payload,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            observability.RecordPublishSuccess(format, message.Destination);
        }
        catch
        {
            observability.RecordPublishFailure(format, message.Destination);
            throw;
        }
    }
}

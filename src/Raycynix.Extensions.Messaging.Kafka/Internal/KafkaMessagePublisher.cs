using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Implementations;
using Raycynix.Extensions.Messaging.Kafka.Interfaces;

namespace Raycynix.Extensions.Messaging.Kafka.Internal;

/// <summary>
/// Publishes Raycynix message envelopes to Kafka topics.
/// </summary>
internal sealed class KafkaMessagePublisher(
    IKafkaProducer producer,
    MessageObservability observability,
    ILogger<KafkaMessagePublisher>? logger = null) : ITransportMessagePublisher
{
    /// <inheritdoc />
    public async ValueTask PublishAsync(SerializedMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var format = message.Format.ToString().ToLowerInvariant();
        using var observation = observability.BeginPublish(format, message.Destination);
        logger?.LogDebug(
            "Publishing Kafka message. Topic={Topic}, Format={Format}, HeaderCount={HeaderCount}.",
            message.Destination,
            message.Format,
            message.Headers.Count);

        try
        {
            var kafkaMessage = new Message<Null, byte[]>
            {
                Value = message.Payload,
                Timestamp = new Timestamp(message.CreatedAt.UtcDateTime),
                Headers = BuildHeaders(message)
            };

            await producer.ProduceAsync(message.Destination, kafkaMessage, cancellationToken).ConfigureAwait(false);
            observability.RecordPublishSuccess(format, message.Destination);
            logger?.LogDebug("Published Kafka message. Topic={Topic}.", message.Destination);
        }
        catch (Exception exception)
        {
            observability.RecordPublishFailure(format, message.Destination);
            logger?.LogWarning(exception, "Kafka message publish failed. Topic={Topic}.", message.Destination);
            throw;
        }
    }

    private static Headers BuildHeaders(SerializedMessage message)
    {
        var headers = new Headers
        {
            { "message-id", System.Text.Encoding.UTF8.GetBytes(message.MessageId) },
            { "content-type", System.Text.Encoding.UTF8.GetBytes(message.ContentType) }
        };

        if (!string.IsNullOrWhiteSpace(message.CorrelationId))
        {
            headers.Add("correlation-id", System.Text.Encoding.UTF8.GetBytes(message.CorrelationId));
        }

        if (!string.IsNullOrWhiteSpace(message.CausationId))
        {
            headers.Add("causation-id", System.Text.Encoding.UTF8.GetBytes(message.CausationId));
        }

        foreach (var header in message.Headers)
        {
            headers.Add(header.Key, System.Text.Encoding.UTF8.GetBytes(header.Value));
        }

        return headers;
    }
}
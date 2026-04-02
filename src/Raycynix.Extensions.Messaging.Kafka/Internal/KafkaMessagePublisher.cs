using Confluent.Kafka;
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
            var kafkaMessage = new Message<Null, byte[]>
            {
                Value = message.Payload,
                Timestamp = new Timestamp(message.CreatedAt.UtcDateTime),
                Headers = BuildHeaders(message)
            };

            await producer.ProduceAsync(message.Destination, kafkaMessage, cancellationToken).ConfigureAwait(false);
            observability.RecordPublishSuccess(format, message.Destination);
        }
        catch
        {
            observability.RecordPublishFailure(format, message.Destination);
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

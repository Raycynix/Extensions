using Confluent.Kafka;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Implementations;

namespace Raycynix.Extensions.Messaging.Kafka.Internal;

/// <summary>
/// Publishes Raycynix message envelopes to Kafka topics.
/// </summary>
internal sealed class KafkaMessagePublisher(
    IMessageSerializer serializer,
    IKafkaProducer producer,
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
            var kafkaMessage = new Message<Null, byte[]>
            {
                Value = serialized.Payload,
                Timestamp = new Timestamp(serialized.CreatedAt.UtcDateTime),
                Headers = BuildHeaders(serialized)
            };

            await producer.ProduceAsync(serialized.Destination, kafkaMessage, cancellationToken).ConfigureAwait(false);
            observability.RecordPublishSuccess(format, envelope.Destination);
        }
        catch
        {
            observability.RecordPublishFailure(format, envelope.Destination);
            throw;
        }
    }

    private static Headers BuildHeaders(SerializedMessage serialized)
    {
        var headers = new Headers
        {
            { "message-id", System.Text.Encoding.UTF8.GetBytes(serialized.MessageId) },
            { "content-type", System.Text.Encoding.UTF8.GetBytes(serialized.ContentType) }
        };

        if (!string.IsNullOrWhiteSpace(serialized.CorrelationId))
        {
            headers.Add("correlation-id", System.Text.Encoding.UTF8.GetBytes(serialized.CorrelationId));
        }

        if (!string.IsNullOrWhiteSpace(serialized.CausationId))
        {
            headers.Add("causation-id", System.Text.Encoding.UTF8.GetBytes(serialized.CausationId));
        }

        foreach (var header in serialized.Headers)
        {
            headers.Add(header.Key, System.Text.Encoding.UTF8.GetBytes(header.Value));
        }

        return headers;
    }
}

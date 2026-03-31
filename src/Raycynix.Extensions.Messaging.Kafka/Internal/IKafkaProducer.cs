using Confluent.Kafka;

namespace Raycynix.Extensions.Messaging.Kafka.Internal;

/// <summary>
/// Minimal Kafka producer abstraction used by the Raycynix Kafka transport.
/// </summary>
internal interface IKafkaProducer : IDisposable
{
    /// <summary>
    /// Publishes a serialized payload to the specified topic.
    /// </summary>
    /// <param name="topic">The Kafka topic.</param>
    /// <param name="message">The Kafka message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ProduceAsync(string topic, Message<Null, byte[]> message, CancellationToken cancellationToken);
}

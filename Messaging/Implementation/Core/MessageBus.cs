using Messaging.Abstractions;

namespace Messaging.Implementation.Core
{
    /// <summary>
    /// Unified Raycynix message bus that can route messages to multiple producers (RabbitMQ, Kafka, etc.).
    /// </summary>
    public class MessageBus(IEnumerable<IMessageProducer> producers) : IMessageBus
    {
        /// <inheritdoc/>
        public async Task PublishAsync<T>(string topic, T message, CancellationToken cancellationToken = default)
        {
            foreach (var producer in producers)
            {
                await producer.PublishAsync(topic, message, cancellationToken);
            }
        }
    }
}

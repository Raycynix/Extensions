using Confluent.Kafka;
using Messaging.Abstractions;
using Messaging.Configurations;

namespace Messaging.Implementation.Kafka
{
    /// <summary>
    /// Kafka producer implementation for Raycynix messaging bus.
    /// </summary>
    public class KafkaProducer(MessagingConfiguration configuration, IMessageSerializer serializer) : IMessageProducer
    {
        private readonly IProducer<Null, string> _producer =
            new ProducerBuilder<Null, string>(
                    new ProducerConfig { BootstrapServers = configuration.Kafka.BootstrapServers })
                .Build();

        /// <inheritdoc/>
        public async Task PublishAsync<T>(string topic, T message, CancellationToken cancellationToken = default)
        {
            var json = serializer.Serialize(message);
            await _producer.ProduceAsync(topic, new Message<Null, string> { Value = json }, cancellationToken);
        }
    }
}

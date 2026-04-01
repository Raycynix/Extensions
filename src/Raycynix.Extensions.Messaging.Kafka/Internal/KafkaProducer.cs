using Confluent.Kafka;
using Raycynix.Extensions.Messaging.Kafka.Configurations;
using Raycynix.Extensions.Messaging.Kafka.Interfaces;

namespace Raycynix.Extensions.Messaging.Kafka.Internal;

/// <summary>
/// Default Kafka producer implementation backed by Confluent.Kafka.
/// </summary>
internal sealed class KafkaProducer : IKafkaProducer
{
    private readonly IProducer<Null, byte[]> _producer;

    /// <summary>
    /// Initializes a new Kafka producer instance.
    /// </summary>
    /// <param name="configuration">The Kafka transport configuration.</param>
    public KafkaProducer(KafkaMessagingConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var producerConfiguration = new ProducerConfig
        {
            BootstrapServers = string.Join(",", configuration.BootstrapServers),
            ClientId = configuration.ClientId,
            EnableIdempotence = configuration.EnableIdempotence,
            Acks = ParseAcks(configuration.Acks)
        };

        _producer = new ProducerBuilder<Null, byte[]>(producerConfiguration).Build();
    }

    /// <inheritdoc />
    public async Task ProduceAsync(string topic, Message<Null, byte[]> message, CancellationToken cancellationToken)
    {
        await _producer.ProduceAsync(topic, message, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }

    private static Acks ParseAcks(string acks)
    {
        return Enum.TryParse<Acks>(acks, ignoreCase: true, out var parsed) ? parsed : Acks.All;
    }
}

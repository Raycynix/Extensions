using Confluent.Kafka;
using Raycynix.Extensions.Messaging.Kafka.Configurations;
using Raycynix.Extensions.Messaging.Kafka.Interfaces;

namespace Raycynix.Extensions.Messaging.Kafka.Internal;

/// <summary>
/// Default Kafka consumer implementation backed by Confluent.Kafka.
/// </summary>
internal sealed class KafkaConsumer : IKafkaConsumer
{
    private readonly IConsumer<Ignore, byte[]> _consumer;

    /// <summary>
    /// Initializes a new Kafka consumer instance.
    /// </summary>
    /// <param name="configuration">The Kafka transport configuration.</param>
    public KafkaConsumer(KafkaMessagingConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var consumerConfiguration = new ConsumerConfig
        {
            BootstrapServers = string.Join(",", configuration.BootstrapServers),
            ClientId = configuration.ClientId,
            GroupId = string.IsNullOrWhiteSpace(configuration.ConsumerGroupId)
                ? "raycynix.extensions.messaging"
                : configuration.ConsumerGroupId,
            EnableAutoCommit = configuration.EnableAutoCommit,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<Ignore, byte[]>(consumerConfiguration).Build();
    }

    /// <inheritdoc />
    public void Subscribe(IEnumerable<string> topics)
    {
        ArgumentNullException.ThrowIfNull(topics);
        _consumer.Subscribe(topics);
    }

    /// <inheritdoc />
    public Task<KafkaIncomingMessage?> ConsumeAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = _consumer.Consume(TimeSpan.FromMilliseconds(50));
            if (result is null)
            {
                return Task.FromResult<KafkaIncomingMessage?>(null);
            }

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string? correlationId = null;
            string? causationId = null;
            string? contentType = null;

            foreach (var header in result.Message.Headers)
            {
                var value = header.GetValueBytes() is { Length: > 0 } bytes
                    ? System.Text.Encoding.UTF8.GetString(bytes)
                    : string.Empty;

                headers[header.Key] = value;

                if (header.Key.Equals("correlation-id", StringComparison.OrdinalIgnoreCase))
                {
                    correlationId = value;
                }

                if (header.Key.Equals("causation-id", StringComparison.OrdinalIgnoreCase))
                {
                    causationId = value;
                }

                if (header.Key.Equals("content-type", StringComparison.OrdinalIgnoreCase))
                {
                    contentType = value;
                }
            }

            return Task.FromResult<KafkaIncomingMessage?>(new KafkaIncomingMessage
            {
                Topic = result.Topic,
                Payload = result.Message.Value,
                MessageId = result.Message.Headers.TryGetLastBytes("message-id", out var messageIdBytes)
                    ? System.Text.Encoding.UTF8.GetString(messageIdBytes)
                    : Guid.NewGuid().ToString("N"),
                CorrelationId = correlationId,
                CausationId = causationId,
                ContentType = contentType,
                Timestamp = result.Message.Timestamp.UtcDateTime == default
                    ? null
                    : new DateTimeOffset(result.Message.Timestamp.UtcDateTime),
                Headers = headers
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
    }

    /// <inheritdoc />
    public void Commit(KafkaIncomingMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        _consumer.Commit();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
    }
}

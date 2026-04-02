namespace Raycynix.Extensions.Messaging.Kafka.Configurations;

/// <summary>
/// Configures Kafka inbound consumption behavior.
/// </summary>
public sealed class KafkaConsumerConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether the Kafka inbound consumer is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the topics consumed by the Kafka inbound consumer.
    /// </summary>
    public string[] Topics { get; set; } = [];

    /// <summary>
    /// Gets or sets the consumer poll interval used when no messages are available.
    /// </summary>
    public int PollIntervalMilliseconds { get; set; } = 100;

    /// <summary>
    /// Validates the Kafka consumer configuration.
    /// </summary>
    public void Validate()
    {
        if (!Enabled)
        {
            return;
        }

        if (Topics.Length == 0 || Topics.All(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException("At least one Kafka consumer topic must be configured.");
        }

        if (PollIntervalMilliseconds < 0)
        {
            throw new InvalidOperationException("Kafka consumer poll interval cannot be negative.");
        }
    }
}

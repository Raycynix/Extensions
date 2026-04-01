namespace Raycynix.Extensions.Messaging.Kafka.Configurations;

/// <summary>
/// Configures Kafka dead-letter publishing for inbound processing failures.
/// </summary>
public sealed class KafkaDeadLetterConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether dead-letter publishing is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the Kafka topic used for dead-letter messages.
    /// </summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>
    /// Validates the dead-letter configuration.
    /// </summary>
    public void Validate()
    {
        if (Enabled && string.IsNullOrWhiteSpace(Topic))
        {
            throw new InvalidOperationException("Kafka dead-letter topic must be configured when dead-lettering is enabled.");
        }
    }
}

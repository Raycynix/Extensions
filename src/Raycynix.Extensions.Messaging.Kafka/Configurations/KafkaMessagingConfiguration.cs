namespace Raycynix.Extensions.Messaging.Kafka.Configurations;

/// <summary>
/// Configures Kafka connectivity and producer behavior.
/// </summary>
public sealed class KafkaMessagingConfiguration
{
    /// <summary>
    /// Gets the list of Kafka bootstrap servers.
    /// </summary>
    public string[] BootstrapServers { get; set; } = [];

    /// <summary>
    /// Gets the Kafka client identifier.
    /// </summary>
    public string ClientId { get; set; } = "raycynix.extensions.messaging";

    /// <summary>
    /// Gets the optional consumer group identifier for future consumer registration.
    /// </summary>
    public string? ConsumerGroupId { get; set; }

    /// <summary>
    /// Gets a value indicating whether producer idempotence is enabled.
    /// </summary>
    public bool EnableIdempotence { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether Kafka auto-commit is enabled for consumers.
    /// </summary>
    public bool EnableAutoCommit { get; set; } = false;

    /// <summary>
    /// Gets the acknowledgment mode.
    /// </summary>
    public string Acks { get; set; } = "all";

    /// <summary>
    /// Validates the Kafka configuration.
    /// </summary>
    public void Validate()
    {
        if (BootstrapServers.Length == 0 || BootstrapServers.All(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException("At least one Kafka bootstrap server must be configured.");
        }
    }
}

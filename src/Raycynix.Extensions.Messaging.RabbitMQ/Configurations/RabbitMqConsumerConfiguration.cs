namespace Raycynix.Extensions.Messaging.RabbitMQ.Configurations;

/// <summary>
/// Configures RabbitMQ inbound message consumption behavior.
/// </summary>
public sealed class RabbitMqConsumerConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether inbound consumer processing is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the polling interval used while waiting for the next delivery.
    /// </summary>
    public int PollIntervalMilliseconds { get; set; } = 250;
}

namespace Raycynix.Extensions.Messaging.RabbitMQ.Configurations;

/// <summary>
/// Configures RabbitMQ dead-letter routing.
/// </summary>
public sealed class RabbitMqDeadLetterConfiguration
{
    /// <summary>
    /// Gets a value indicating whether dead-letter routing is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets the dead-letter exchange name.
    /// </summary>
    public string Exchange { get; set; } = "raycynix.dlx";

    /// <summary>
    /// Gets the dead-letter queue name.
    /// </summary>
    public string Queue { get; set; } = "raycynix.dlq";

    /// <summary>
    /// Gets the dead-letter routing key.
    /// </summary>
    public string RoutingKey { get; set; } = "dead-letter";
}

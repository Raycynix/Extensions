namespace Raycynix.Extensions.Messaging.RabbitMQ.Configurations;

/// <summary>
/// Configures the primary RabbitMQ exchange.
/// </summary>
public sealed class RabbitMqExchangeConfiguration
{
    /// <summary>
    /// Gets the exchange name.
    /// </summary>
    public string Name { get; set; } = "raycynix.events";

    /// <summary>
    /// Gets the exchange type.
    /// </summary>
    public string Type { get; set; } = "topic";

    /// <summary>
    /// Gets a value indicating whether the exchange is durable.
    /// </summary>
    public bool Durable { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether the exchange is auto-deleted.
    /// </summary>
    public bool AutoDelete { get; set; }
}

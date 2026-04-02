namespace Raycynix.Extensions.Messaging.RabbitMQ.Configurations;

/// <summary>
/// Configures the primary RabbitMQ queue.
/// </summary>
public sealed class RabbitMqQueueConfiguration
{
    /// <summary>
    /// Gets the queue name.
    /// </summary>
    public string Name { get; set; } = "raycynix.events";

    /// <summary>
    /// Gets a value indicating whether the queue is durable.
    /// </summary>
    public bool Durable { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether the queue is exclusive.
    /// </summary>
    public bool Exclusive { get; set; }

    /// <summary>
    /// Gets a value indicating whether the queue is auto-deleted.
    /// </summary>
    public bool AutoDelete { get; set; }

    /// <summary>
    /// Gets the channel prefetch count.
    /// </summary>
    public ushort PrefetchCount { get; set; } = 16;

    /// <summary>
    /// Gets the optional per-message TTL in milliseconds.
    /// </summary>
    public int? MessageTtlMilliseconds { get; set; }
}

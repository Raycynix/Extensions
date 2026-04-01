namespace Raycynix.Extensions.Messaging.RabbitMQ.Internal;

/// <summary>
/// Represents a RabbitMQ delivery retrieved for inbound processing.
/// </summary>
public sealed record RabbitMqIncomingDelivery
{
    /// <summary>
    /// Gets the broker delivery tag.
    /// </summary>
    public required ulong DeliveryTag { get; init; }

    /// <summary>
    /// Gets the routing key used for the delivery.
    /// </summary>
    public required string RoutingKey { get; init; }

    /// <summary>
    /// Gets the message payload.
    /// </summary>
    public required ReadOnlyMemory<byte> Body { get; init; }

    /// <summary>
    /// Gets the message identifier.
    /// </summary>
    public string? MessageId { get; init; }

    /// <summary>
    /// Gets the correlation identifier.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Gets the content type.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Gets the message timestamp.
    /// </summary>
    public DateTimeOffset? Timestamp { get; init; }

    /// <summary>
    /// Gets the message headers.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

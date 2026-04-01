namespace Raycynix.Extensions.Messaging.Kafka.Interfaces;

/// <summary>
/// Represents a Kafka message delivered to the transport consumer.
/// </summary>
public sealed record KafkaIncomingMessage
{
    /// <summary>
    /// Gets the Kafka topic.
    /// </summary>
    public required string Topic { get; init; }

    /// <summary>
    /// Gets the payload bytes.
    /// </summary>
    public required byte[] Payload { get; init; }

    /// <summary>
    /// Gets the message identifier.
    /// </summary>
    public required string MessageId { get; init; }

    /// <summary>
    /// Gets the optional correlation identifier.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Gets the optional causation identifier.
    /// </summary>
    public string? CausationId { get; init; }

    /// <summary>
    /// Gets the content type.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Gets the message timestamp.
    /// </summary>
    public DateTimeOffset? Timestamp { get; init; }

    /// <summary>
    /// Gets the transport headers.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

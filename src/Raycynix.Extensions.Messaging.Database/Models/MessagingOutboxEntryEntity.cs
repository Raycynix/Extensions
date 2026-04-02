namespace Raycynix.Extensions.Messaging.Database.Models;

/// <summary>
/// Represents a persisted outbox entry for outgoing transport messages.
/// </summary>
public sealed class MessagingOutboxEntryEntity
{
    /// <summary>
    /// Gets or sets the message identifier.
    /// </summary>
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the destination.
    /// </summary>
    public string Destination { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the serialized payload bytes.
    /// </summary>
    public byte[] Payload { get; set; } = [];

    /// <summary>
    /// Gets or sets the payload format.
    /// </summary>
    public int Format { get; set; }

    /// <summary>
    /// Gets or sets the content type.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the correlation identifier.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the causation identifier.
    /// </summary>
    public string? CausationId { get; set; }

    /// <summary>
    /// Gets or sets the original message creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the serialized headers payload.
    /// </summary>
    public string Headers { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the outbox status.
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the dispatch attempt count.
    /// </summary>
    public int AttemptCount { get; set; }

    /// <summary>
    /// Gets or sets the next retry timestamp.
    /// </summary>
    public DateTimeOffset NextAttemptAt { get; set; }

    /// <summary>
    /// Gets or sets the latest publishing error.
    /// </summary>
    public string? Error { get; set; }
}

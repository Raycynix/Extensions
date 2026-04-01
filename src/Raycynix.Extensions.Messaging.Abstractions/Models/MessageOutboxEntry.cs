namespace Raycynix.Extensions.Messaging.Abstractions.Models;

/// <summary>
/// Represents persisted outbox state for a single outgoing message.
/// </summary>
public sealed record MessageOutboxEntry
{
    /// <summary>
    /// Gets the serialized message payload and metadata.
    /// </summary>
    public required SerializedMessage Message { get; init; }

    /// <summary>
    /// Gets the current outbox status.
    /// </summary>
    public required MessageOutboxStatus Status { get; init; }

    /// <summary>
    /// Gets the timestamp when the message was first added to the outbox.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets the timestamp of the latest outbox update.
    /// </summary>
    public required DateTimeOffset UpdatedAt { get; init; }

    /// <summary>
    /// Gets the number of dispatch attempts that have already been performed.
    /// </summary>
    public required int AttemptCount { get; init; }

    /// <summary>
    /// Gets the next moment when the message is eligible for retry.
    /// </summary>
    public required DateTimeOffset NextAttemptAt { get; init; }

    /// <summary>
    /// Gets the latest publish error, when available.
    /// </summary>
    public string? Error { get; init; }
}

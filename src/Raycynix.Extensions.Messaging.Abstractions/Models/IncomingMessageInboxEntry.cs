namespace Raycynix.Extensions.Messaging.Abstractions.Models;

/// <summary>
/// Represents persisted inbox state for a single incoming message identifier.
/// </summary>
public sealed record IncomingMessageInboxEntry
{
    /// <summary>
    /// Gets the message identifier.
    /// </summary>
    public required string MessageId { get; init; }

    /// <summary>
    /// Gets the current inbox status.
    /// </summary>
    public required IncomingMessageInboxStatus Status { get; init; }

    /// <summary>
    /// Gets the message destination.
    /// </summary>
    public required string Destination { get; init; }

    /// <summary>
    /// Gets the timestamp of the latest status update.
    /// </summary>
    public required DateTimeOffset UpdatedAt { get; init; }

    /// <summary>
    /// Gets the latest processing error, when available.
    /// </summary>
    public string? Error { get; init; }
}

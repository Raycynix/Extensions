using Raycynix.Extensions.Messaging.Abstractions.Enums;

namespace Raycynix.Extensions.Messaging.Abstractions.Models;

/// <summary>
/// Represents an envelope serialized into transport-ready bytes.
/// </summary>
public sealed record SerializedMessage
{
    /// <summary>
    /// Gets the logical destination.
    /// </summary>
    public required string Destination { get; init; }

    /// <summary>
    /// Gets the payload bytes.
    /// </summary>
    public required byte[] Payload { get; init; }

    /// <summary>
    /// Gets the payload format.
    /// </summary>
    public required MessageFormat Format { get; init; }

    /// <summary>
    /// Gets the content type.
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Gets the message identifier.
    /// </summary>
    public required string MessageId { get; init; }

    /// <summary>
    /// Gets the correlation identifier.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Gets the causation identifier.
    /// </summary>
    public string? CausationId { get; init; }

    /// <summary>
    /// Gets the message timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets the headers.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

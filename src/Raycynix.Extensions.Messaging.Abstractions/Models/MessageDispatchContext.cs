using Raycynix.Extensions.Contracts.Models;

namespace Raycynix.Extensions.Messaging.Abstractions.Models;

/// <summary>
/// Represents normalized metadata for an incoming message dispatch operation.
/// </summary>
public sealed record MessageDispatchContext
{
    /// <summary>
    /// Gets the payload type.
    /// </summary>
    public required Type MessageType { get; init; }

    /// <summary>
    /// Gets the logical destination.
    /// </summary>
    public required string Destination { get; init; }

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
    /// Gets the resolved contract metadata.
    /// </summary>
    public ContractMetadata? Contract { get; init; }

    /// <summary>
    /// Gets the message creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
}

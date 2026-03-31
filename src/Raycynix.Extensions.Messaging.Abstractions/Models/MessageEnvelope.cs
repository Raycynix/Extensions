using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Contracts.Models;

namespace Raycynix.Extensions.Messaging.Abstractions.Models;

/// <summary>
/// Represents a transport-agnostic outgoing or incoming message envelope.
/// </summary>
/// <typeparam name="TMessage">The payload type.</typeparam>
public sealed record MessageEnvelope<TMessage>
{
    /// <summary>
    /// Gets the payload.
    /// </summary>
    public required TMessage Message { get; init; }

    /// <summary>
    /// Gets the logical destination.
    /// </summary>
    public required string Destination { get; init; }

    /// <summary>
    /// Gets the payload format.
    /// </summary>
    public required MessageFormat Format { get; init; }

    /// <summary>
    /// Gets the unique message identifier.
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
    /// Gets the message creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the contract metadata for the payload.
    /// </summary>
    public ContractMetadata? Contract { get; init; }

    /// <summary>
    /// Gets the optional headers.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

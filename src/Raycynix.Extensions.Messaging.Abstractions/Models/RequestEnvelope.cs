using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Contracts.Models;

namespace Raycynix.Extensions.Messaging.Abstractions.Models;

/// <summary>
/// Represents a transport-agnostic direct request envelope.
/// </summary>
/// <typeparam name="TRequest">The request payload type.</typeparam>
public sealed record RequestEnvelope<TRequest>
{
    /// <summary>
    /// Gets the request payload.
    /// </summary>
    public required TRequest Request { get; init; }

    /// <summary>
    /// Gets the logical route, path, or method name.
    /// </summary>
    public required string Destination { get; init; }

    /// <summary>
    /// Gets the request payload format.
    /// </summary>
    public required MessageFormat Format { get; init; }

    /// <summary>
    /// Gets the request identifier.
    /// </summary>
    public required string RequestId { get; init; }

    /// <summary>
    /// Gets the correlation identifier.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Gets the request creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the request timeout.
    /// </summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Gets the contract metadata for the request payload.
    /// </summary>
    public ContractMetadata? Contract { get; init; }

    /// <summary>
    /// Gets the request headers.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

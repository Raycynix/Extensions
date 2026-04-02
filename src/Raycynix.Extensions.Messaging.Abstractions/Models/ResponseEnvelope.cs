namespace Raycynix.Extensions.Messaging.Abstractions.Models;

/// <summary>
/// Represents a transport-agnostic direct response envelope.
/// </summary>
/// <typeparam name="TResponse">The response payload type.</typeparam>
public sealed record ResponseEnvelope<TResponse>
{
    /// <summary>
    /// Gets the response payload.
    /// </summary>
    public required TResponse Response { get; init; }

    /// <summary>
    /// Gets the numeric transport status code when available.
    /// </summary>
    public int StatusCode { get; init; } = 200;

    /// <summary>
    /// Gets the correlation identifier.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Gets the response headers.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

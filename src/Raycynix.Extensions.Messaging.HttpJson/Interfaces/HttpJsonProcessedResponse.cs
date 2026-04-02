using System.Net;

namespace Raycynix.Extensions.Messaging.HttpJson.Interfaces;

/// <summary>
/// Represents a transport-ready HTTP JSON response produced by the request processor.
/// </summary>
public sealed record HttpJsonProcessedResponse
{
    /// <summary>
    /// Gets the HTTP status code.
    /// </summary>
    public required HttpStatusCode StatusCode { get; init; }

    /// <summary>
    /// Gets the serialized response payload.
    /// </summary>
    public byte[] Payload { get; init; } = [];

    /// <summary>
    /// Gets the response content type.
    /// </summary>
    public string ContentType { get; init; } = "application/json";

    /// <summary>
    /// Gets the response headers.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

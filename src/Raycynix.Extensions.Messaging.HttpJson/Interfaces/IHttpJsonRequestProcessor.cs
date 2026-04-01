namespace Raycynix.Extensions.Messaging.HttpJson.Interfaces;

/// <summary>
/// Processes inbound HTTP JSON direct requests through the shared messaging request pipeline.
/// </summary>
public interface IHttpJsonRequestProcessor
{
    /// <summary>
    /// Processes an inbound HTTP JSON request and returns a transport-ready response.
    /// </summary>
    /// <typeparam name="TRequest">The request payload type.</typeparam>
    /// <typeparam name="TResponse">The response payload type.</typeparam>
    /// <param name="destination">The logical destination or route.</param>
    /// <param name="payload">The serialized request payload.</param>
    /// <param name="headers">The inbound headers.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The transport-ready HTTP JSON response.</returns>
    ValueTask<HttpJsonProcessedResponse> ProcessAsync<TRequest, TResponse>(
        string destination,
        ReadOnlyMemory<byte> payload,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);
}

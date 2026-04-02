using Raycynix.Extensions.Messaging.Abstractions.Models;

namespace Raycynix.Extensions.Messaging.Abstractions.Interfaces;

/// <summary>
/// Sends direct request/response messages between services.
/// </summary>
public interface IRequestClient
{
    /// <summary>
    /// Sends a request envelope and returns a response envelope.
    /// </summary>
    /// <typeparam name="TRequest">The request payload type.</typeparam>
    /// <typeparam name="TResponse">The response payload type.</typeparam>
    /// <param name="request">The outgoing request envelope.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response envelope.</returns>
    ValueTask<ResponseEnvelope<TResponse>> SendAsync<TRequest, TResponse>(
        RequestEnvelope<TRequest> request,
        CancellationToken cancellationToken = default);
}

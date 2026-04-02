using Raycynix.Extensions.Messaging.Abstractions.Models;

namespace Raycynix.Extensions.Messaging.Abstractions.Interfaces;

/// <summary>
/// Dispatches direct request envelopes to registered request handlers.
/// </summary>
public interface IRequestDispatcher
{
    /// <summary>
    /// Dispatches the supplied request envelope to the matching request handler.
    /// </summary>
    /// <typeparam name="TRequest">The request payload type.</typeparam>
    /// <typeparam name="TResponse">The response payload type.</typeparam>
    /// <param name="request">The request envelope to dispatch.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The produced response envelope.</returns>
    ValueTask<ResponseEnvelope<TResponse>> DispatchAsync<TRequest, TResponse>(
        RequestEnvelope<TRequest> request,
        CancellationToken cancellationToken = default);
}

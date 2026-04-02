using Raycynix.Extensions.Messaging.Abstractions.Models;

namespace Raycynix.Extensions.Messaging.Abstractions.Interfaces;

/// <summary>
/// Handles direct request/response messages for a specific request and response type pair.
/// </summary>
/// <typeparam name="TRequest">The request payload type.</typeparam>
/// <typeparam name="TResponse">The response payload type.</typeparam>
public interface IRequestHandler<TRequest, TResponse>
{
    /// <summary>
    /// Handles the supplied request envelope and produces a response envelope.
    /// </summary>
    /// <param name="request">The request envelope.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The produced response envelope.</returns>
    ValueTask<ResponseEnvelope<TResponse>> HandleAsync(
        RequestEnvelope<TRequest> request,
        CancellationToken cancellationToken = default);
}

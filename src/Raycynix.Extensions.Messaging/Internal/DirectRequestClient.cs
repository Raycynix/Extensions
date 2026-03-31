using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;

namespace Raycynix.Extensions.Messaging.Internal;

internal sealed class DirectRequestClient : IDirectRequestClient
{
    public ValueTask<ResponseEnvelope<TResponse>> SendAsync<TRequest, TResponse>(
        RequestEnvelope<TRequest> request,
        CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException(
            "No direct request transport is configured. Register Raycynix.Extensions.Messaging.HttpJson or Raycynix.Extensions.Messaging.Grpc.");
    }
}

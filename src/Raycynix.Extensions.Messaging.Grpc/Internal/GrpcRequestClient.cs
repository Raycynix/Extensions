using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Grpc.Configurations;
using Raycynix.Extensions.Messaging.Grpc.Interfaces;

namespace Raycynix.Extensions.Messaging.Grpc.Internal;

internal sealed class GrpcRequestClient(
    IEnumerable<IGrpcRequestOperation> operations,
    IGrpcClientFactory clientFactory,
    GrpcDirectMessagingConfiguration configuration,
    ILogger<GrpcRequestClient>? logger = null) : IGrpcRequestClient
{
    public async ValueTask<ResponseEnvelope<TResponse>> SendAsync<TRequest, TResponse>(
        RequestEnvelope<TRequest> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var operation = operations.FirstOrDefault(candidate =>
            string.Equals(candidate.Destination, request.Destination, StringComparison.OrdinalIgnoreCase) &&
            candidate.RequestType == typeof(TRequest) &&
            candidate.ResponseType == typeof(TResponse));

        if (operation is null)
        {
            logger?.LogWarning(
                "No gRPC request operation is registered. Destination={Destination}, RequestType={RequestType}, ResponseType={ResponseType}.",
                request.Destination,
                typeof(TRequest).FullName,
                typeof(TResponse).FullName);

            throw new InvalidOperationException(
                $"No gRPC operation is registered for destination '{request.Destination}' and types '{typeof(TRequest).FullName}'/'{typeof(TResponse).FullName}'.");
        }

        logger?.LogDebug(
            "Sending gRPC request. Destination={Destination}, RequestType={RequestType}, ResponseType={ResponseType}.",
            request.Destination,
            typeof(TRequest).FullName,
            typeof(TResponse).FullName);
        using var clientHandle = clientFactory.Create(operation.ClientType, configuration.Address);
        var response = await operation.InvokeAsync(clientHandle.Client, request.Request!, cancellationToken)
            .ConfigureAwait(false);
        logger?.LogDebug("gRPC request completed. Destination={Destination}.", request.Destination);

        return new ResponseEnvelope<TResponse>
        {
            Response = (TResponse)response,
            StatusCode = 200,
            CorrelationId = request.CorrelationId,
            Headers = request.Headers
        };
    }
}
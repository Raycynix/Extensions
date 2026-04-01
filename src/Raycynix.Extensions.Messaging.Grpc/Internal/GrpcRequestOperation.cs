using Raycynix.Extensions.Messaging.Grpc.Interfaces;

namespace Raycynix.Extensions.Messaging.Grpc.Internal;

internal sealed class GrpcRequestOperation<TGrpcClient, TRequest, TResponse>(
    string destination,
    Func<TGrpcClient, TRequest, CancellationToken, Task<TResponse>> send) : IGrpcRequestOperation
{
    public string Destination { get; } = destination;

    public Type ClientType => typeof(TGrpcClient);

    public Type RequestType => typeof(TRequest);

    public Type ResponseType => typeof(TResponse);

    public async Task<object> InvokeAsync(object client, object request, CancellationToken cancellationToken)
    {
        return await send((TGrpcClient)client, (TRequest)request, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("gRPC operation returned a null response.");
    }
}

namespace Raycynix.Extensions.Messaging.Grpc.Internal;

internal interface IGrpcRequestOperation
{
    string Destination { get; }

    Type ClientType { get; }

    Type RequestType { get; }

    Type ResponseType { get; }

    Task<object> InvokeAsync(object client, object request, CancellationToken cancellationToken);
}

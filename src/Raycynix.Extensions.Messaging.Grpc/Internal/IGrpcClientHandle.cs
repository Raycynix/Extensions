namespace Raycynix.Extensions.Messaging.Grpc.Internal;

internal interface IGrpcClientHandle : IDisposable
{
    object Client { get; }
}

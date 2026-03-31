namespace Raycynix.Extensions.Messaging.Grpc.Internal;

internal interface IGrpcClientFactory
{
    IGrpcClientHandle Create(Type clientType, string address);
}

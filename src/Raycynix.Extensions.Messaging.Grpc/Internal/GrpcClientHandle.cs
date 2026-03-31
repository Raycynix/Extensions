using Grpc.Net.Client;

namespace Raycynix.Extensions.Messaging.Grpc.Internal;

internal sealed class GrpcClientHandle(GrpcChannel channel, object client) : IGrpcClientHandle
{
    public object Client { get; } = client;

    public void Dispose()
    {
        channel.Dispose();
    }
}

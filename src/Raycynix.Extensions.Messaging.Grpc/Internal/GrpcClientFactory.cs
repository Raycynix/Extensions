using System.Reflection;
using Grpc.Core;
using Grpc.Net.Client;

namespace Raycynix.Extensions.Messaging.Grpc.Internal;

internal sealed class GrpcClientFactory : IGrpcClientFactory
{
    public IGrpcClientHandle Create(Type clientType, string address)
    {
        var channel = GrpcChannel.ForAddress(address);
        var constructor = clientType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, [typeof(CallInvoker)]);

        if (constructor is null)
        {
            channel.Dispose();
            throw new InvalidOperationException(
                $"gRPC client type '{clientType.FullName}' must expose a public constructor accepting CallInvoker.");
        }

        var client = constructor.Invoke([channel.CreateCallInvoker()]);
        return new GrpcClientHandle(channel, client);
    }
}

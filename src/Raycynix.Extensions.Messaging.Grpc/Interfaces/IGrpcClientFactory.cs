namespace Raycynix.Extensions.Messaging.Grpc.Interfaces;

/// <summary>
/// Creates strongly typed gRPC client handles for direct request operations.
/// </summary>
public interface IGrpcClientFactory
{
    /// <summary>
    /// Creates a gRPC client handle for the specified client type and endpoint address.
    /// </summary>
    /// <param name="clientType">The generated gRPC client type.</param>
    /// <param name="address">The remote service address.</param>
    /// <returns>A disposable client handle.</returns>
    IGrpcClientHandle Create(Type clientType, string address);
}

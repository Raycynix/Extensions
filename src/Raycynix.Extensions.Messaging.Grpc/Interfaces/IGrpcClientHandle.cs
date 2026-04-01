namespace Raycynix.Extensions.Messaging.Grpc.Interfaces;

/// <summary>
/// Wraps a created gRPC client instance and its lifetime.
/// </summary>
public interface IGrpcClientHandle : IDisposable
{
    /// <summary>
    /// Gets the created gRPC client instance.
    /// </summary>
    object Client { get; }
}

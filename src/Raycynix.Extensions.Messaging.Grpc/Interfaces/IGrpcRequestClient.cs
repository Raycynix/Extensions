using Raycynix.Extensions.Messaging.Abstractions.Interfaces;

namespace Raycynix.Extensions.Messaging.Grpc.Interfaces;

/// <summary>
/// Sends direct request/response calls over gRPC.
/// </summary>
public interface IGrpcRequestClient : IDirectRequestClient;

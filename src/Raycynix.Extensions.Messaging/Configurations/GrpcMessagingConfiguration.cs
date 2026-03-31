namespace Raycynix.Extensions.Messaging.Configurations;

/// <summary>
/// Configures default gRPC/protobuf message settings.
/// </summary>
public sealed class GrpcMessagingConfiguration
{
    /// <summary>
    /// Gets the content type used for gRPC/protobuf payloads.
    /// </summary>
    public string ContentType { get; set; } = "application/grpc+proto";
}

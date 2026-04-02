namespace Raycynix.Extensions.Messaging.Abstractions.Enums;

/// <summary>
/// Defines the payload format used for a message.
/// </summary>
public enum MessageFormat
{
    /// <summary>
    /// Indicates a JSON-serialized message payload.
    /// </summary>
    Json = 0,

    /// <summary>
    /// Indicates a gRPC or protobuf-encoded message payload.
    /// </summary>
    Grpc = 1
}

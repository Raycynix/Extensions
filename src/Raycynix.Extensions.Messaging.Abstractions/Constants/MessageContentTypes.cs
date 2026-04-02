namespace Raycynix.Extensions.Messaging.Abstractions.Constants;

/// <summary>
/// Common content types used by Raycynix messaging codecs.
/// </summary>
public static class MessageContentTypes
{
    /// <summary>
    /// Default JSON payload content type.
    /// </summary>
    public const string Json = "application/json";

    /// <summary>
    /// Default protobuf payload content type used for gRPC-style binary messages.
    /// </summary>
    public const string Grpc = "application/grpc+proto";
}

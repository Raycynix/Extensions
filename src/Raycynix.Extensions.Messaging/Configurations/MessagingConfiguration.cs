using Raycynix.Extensions.Messaging.Abstractions.Enums;

namespace Raycynix.Extensions.Messaging.Configurations;

/// <summary>
/// Configures transport-agnostic messaging behavior.
/// </summary>
public sealed class MessagingConfiguration
{
    /// <summary>
    /// Gets the default payload format for outgoing messages.
    /// </summary>
    public MessageFormat DefaultFormat { get; set; } = MessageFormat.Json;

    /// <summary>
    /// Gets a value indicating whether missing correlation identifiers should be auto-generated.
    /// </summary>
    public bool AutoGenerateCorrelationId { get; set; } = true;

    /// <summary>
    /// Gets the JSON message configuration.
    /// </summary>
    public JsonMessagingConfiguration Json { get; set; } = new();

    /// <summary>
    /// Gets the gRPC message configuration.
    /// </summary>
    public GrpcMessagingConfiguration Grpc { get; set; } = new();

    /// <summary>
    /// Validates the messaging configuration.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Grpc.ContentType))
        {
            throw new InvalidOperationException("gRPC content type cannot be empty.");
        }
    }
}

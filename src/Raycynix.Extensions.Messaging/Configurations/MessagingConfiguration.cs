using Raycynix.Extensions.Messaging.Abstractions.Enums;

namespace Raycynix.Extensions.Messaging.Configurations;

/// <summary>
/// Configures transport-agnostic messaging behavior.
/// </summary>
public sealed class MessagingConfiguration
{
    /// <summary>
    /// Gets or sets the logical source name stamped onto outgoing messages and requests.
    /// </summary>
    public string? SourceName { get; set; }

    /// <summary>
    /// Gets the default payload format for outgoing messages.
    /// </summary>
    public MessageFormat DefaultFormat { get; set; } = MessageFormat.Json;

    /// <summary>
    /// Gets a value indicating whether missing correlation identifiers should be auto-generated.
    /// </summary>
    public bool AutoGenerateCorrelationId { get; set; } = true;

    /// <summary>
    /// Gets the incoming message processing configuration.
    /// </summary>
    public IncomingMessageProcessingConfiguration IncomingProcessing { get; set; } = new();

    /// <summary>
    /// Gets the dispatch retry configuration.
    /// </summary>
    public MessageDispatchRetryConfiguration DispatchRetry { get; set; } = new();

    /// <summary>
    /// Gets the outgoing outbox configuration.
    /// </summary>
    public MessageOutboxConfiguration Outbox { get; set; } = new();

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
        if (IncomingProcessing.TrustedSources.Any(static source => string.IsNullOrWhiteSpace(source)))
        {
            throw new InvalidOperationException("Incoming trusted message sources cannot contain empty values.");
        }

        if (string.IsNullOrWhiteSpace(Grpc.ContentType))
        {
            throw new InvalidOperationException("gRPC content type cannot be empty.");
        }

        DispatchRetry.Validate();
        Outbox.Validate();
    }
}

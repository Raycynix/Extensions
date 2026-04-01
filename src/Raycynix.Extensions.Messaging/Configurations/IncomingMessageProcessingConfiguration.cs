namespace Raycynix.Extensions.Messaging.Configurations;

/// <summary>
/// Configures deduplication and idempotent processing behavior for incoming messages.
/// </summary>
public sealed class IncomingMessageProcessingConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether duplicate incoming messages should be ignored once processed.
    /// </summary>
    public bool EnableDeduplication { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether successfully processed message identifiers should be remembered.
    /// </summary>
    public bool EnableIdempotency { get; set; } = true;
}

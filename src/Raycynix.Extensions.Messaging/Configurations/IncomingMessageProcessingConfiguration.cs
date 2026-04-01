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

    /// <summary>
    /// Gets or sets a value indicating whether inbound security headers should be validated before dispatch.
    /// </summary>
    public bool ValidateSecurityHeaders { get; set; } = true;

    /// <summary>
    /// Gets or sets the list of trusted inbound message sources.
    /// </summary>
    public List<string> TrustedSources { get; set; } = [];
}

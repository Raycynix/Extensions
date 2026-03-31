namespace Raycynix.Extensions.Messaging.HttpJson.Configurations;

/// <summary>
/// Configures the direct HTTP JSON transport.
/// </summary>
public sealed class HttpJsonMessagingConfiguration
{
    /// <summary>
    /// Gets the base service address.
    /// </summary>
    public string BaseAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets the request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Validates the HTTP JSON configuration.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(BaseAddress))
        {
            throw new InvalidOperationException("HTTP JSON base address cannot be empty.");
        }

        if (!Uri.TryCreate(BaseAddress, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("HTTP JSON base address must be an absolute URI.");
        }

        if (TimeoutSeconds <= 0)
        {
            throw new InvalidOperationException("HTTP JSON timeout must be greater than zero.");
        }
    }
}

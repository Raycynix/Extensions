namespace Raycynix.Extensions.Secrets.Options;

/// <summary>
/// Configures secret-resolution behavior for the default provider chain.
/// </summary>
public sealed class SecretOptions
{
    /// <summary>
    /// Gets the ordered provider types to prefer when resolving secrets.
    /// Providers not listed here are evaluated afterward in registration order.
    /// </summary>
    public IList<string> ProviderOrder { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether resolution continues with the next provider when a provider fails.
    /// </summary>
    public bool ContinueOnProviderError { get; set; } = true;
}

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
}

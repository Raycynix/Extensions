namespace Raycynix.Extensions.Secrets;

/// <summary>
/// Provides the stable names of the built-in secret providers.
/// </summary>
public static class SecretProviderNames
{
    /// <summary>
    /// Identifies the provider backed by <c>IConfiguration</c>.
    /// </summary>
    public const string Configuration = "Configuration";

    /// <summary>
    /// Identifies the provider that uses exact environment variable keys.
    /// </summary>
    public const string Environment = "Environment";

    /// <summary>
    /// Identifies the GitHub Actions-style environment provider.
    /// </summary>
    public const string GitHub = "GitHub";

    /// <summary>
    /// Identifies the TeamCity-style environment provider.
    /// </summary>
    public const string TeamCity = "TeamCity";
}

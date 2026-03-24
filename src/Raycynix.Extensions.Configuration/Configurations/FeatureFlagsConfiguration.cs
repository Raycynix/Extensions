namespace Raycynix.Extensions.Configuration.Configurations;

/// <summary>
/// Represents the standard Raycynix feature flags configuration section.
/// </summary>
public sealed class FeatureFlagsConfiguration
{
    /// <summary>
    /// Gets or sets the configured feature flags.
    /// </summary>
    public Dictionary<string, bool> Flags { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

namespace Raycynix.Extensions.Configuration.Configurations;

/// <summary>
/// Configures Raycynix configuration diagnostics behavior.
/// </summary>
public sealed class ConfigurationDiagnosticsOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether redacted configuration snapshots can be requested.
    /// </summary>
    public bool EnableSnapshots { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of reload results retained per options type and name.
    /// </summary>
    /// <remarks>
    /// Values lower than one are treated as one by the diagnostics store.
    /// </remarks>
    public int MaxReloadHistoryPerOptions { get; set; } = 1;
}

namespace Raycynix.Extensions.Configuration.AspNetCore.FeatureGate;

/// <summary>
/// Defines how a feature gate evaluates multiple feature flags.
/// </summary>
public enum FeatureGateMode
{
    /// <summary>
    /// Requires all feature flags to be enabled.
    /// </summary>
    All = 0,

    /// <summary>
    /// Requires at least one feature flag to be enabled.
    /// </summary>
    Any = 1
}

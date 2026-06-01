using Microsoft.AspNetCore.Http;

namespace Raycynix.Extensions.Configuration.AspNetCore.FeatureGate;

/// <summary>
/// Configures HTTP responses produced by the Raycynix feature gate middleware.
/// </summary>
public sealed class FeatureGateOptions
{
    /// <summary>
    /// Gets or sets the status code returned when a required feature flag is disabled.
    /// </summary>
    public int DisabledStatusCode { get; set; } = StatusCodes.Status404NotFound;

    /// <summary>
    /// Gets or sets the status code returned when a gated endpoint is reached without a registered feature flag accessor.
    /// </summary>
    public int MissingFeatureFlagAccessorStatusCode { get; set; } = StatusCodes.Status404NotFound;
}

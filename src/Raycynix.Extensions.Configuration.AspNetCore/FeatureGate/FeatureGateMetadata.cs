namespace Raycynix.Extensions.Configuration.AspNetCore.FeatureGate;

/// <summary>
/// Represents feature flag requirements attached to an ASP.NET Core endpoint.
/// </summary>
public sealed class FeatureGateMetadata
{
    private FeatureGateMetadata(IReadOnlyCollection<string> featureFlags, FeatureGateMode mode)
    {
        FeatureFlags = featureFlags;
        Mode = mode;
    }

    /// <summary>
    /// Gets the required feature flags.
    /// </summary>
    public IReadOnlyCollection<string> FeatureFlags { get; }

    /// <summary>
    /// Gets the feature gate evaluation mode.
    /// </summary>
    public FeatureGateMode Mode { get; }

    /// <summary>
    /// Creates metadata that requires all provided feature flags to be enabled.
    /// </summary>
    /// <param name="featureFlags">The feature flag names.</param>
    /// <returns>The created feature gate metadata.</returns>
    public static FeatureGateMetadata CreateAll(params string[] featureFlags)
    {
        return new FeatureGateMetadata(Normalize(featureFlags), FeatureGateMode.All);
    }

    /// <summary>
    /// Creates metadata that requires at least one of the provided feature flags to be enabled.
    /// </summary>
    /// <param name="featureFlags">The feature flag names.</param>
    /// <returns>The created feature gate metadata.</returns>
    public static FeatureGateMetadata CreateAny(params string[] featureFlags)
    {
        return new FeatureGateMetadata(Normalize(featureFlags), FeatureGateMode.Any);
    }

    private static IReadOnlyCollection<string> Normalize(IEnumerable<string> featureFlags)
    {
        ArgumentNullException.ThrowIfNull(featureFlags);

        var normalized = featureFlags
            .Where(static flag => !string.IsNullOrWhiteSpace(flag))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (normalized.Length == 0)
        {
            throw new ArgumentException("At least one feature flag must be provided.", nameof(featureFlags));
        }

        return normalized;
    }
}

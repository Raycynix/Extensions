namespace Raycynix.Extensions.Configuration.AspNetCore.FeatureGate;

/// <summary>
/// Declares feature flag requirements for ASP.NET Core endpoints and MVC actions.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class FeatureGateAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureGateAttribute"/> class that requires all provided feature flags.
    /// </summary>
    /// <param name="featureFlags">The required feature flag names.</param>
    public FeatureGateAttribute(params string[] featureFlags)
        : this(FeatureGateMode.All, featureFlags)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureGateAttribute"/> class with an explicit evaluation mode.
    /// </summary>
    /// <param name="mode">The feature gate evaluation mode.</param>
    /// <param name="featureFlags">The required feature flag names.</param>
    public FeatureGateAttribute(FeatureGateMode mode, params string[] featureFlags)
    {
        Metadata = mode == FeatureGateMode.Any
            ? FeatureGateMetadata.CreateAny(featureFlags)
            : FeatureGateMetadata.CreateAll(featureFlags);
    }

    /// <summary>
    /// Gets the normalized feature gate metadata represented by the attribute.
    /// </summary>
    internal FeatureGateMetadata Metadata { get; }
}

using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Configurations;

namespace Raycynix.Extensions.Configuration.Implementations;

/// <summary>
/// Provides typed access to the current feature-flag snapshot.
/// </summary>
internal sealed class FeatureFlagAccessor(
    IConfigurationAccessor<FeatureFlagsConfiguration> configurationAccessor) : IFeatureFlagAccessor
{
    /// <inheritdoc />
    public bool IsEnabled(string flagName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(flagName);

        return configurationAccessor.Current.Flags.TryGetValue(flagName, out var enabled) && enabled;
    }

    /// <inheritdoc />
    public bool IsDisabled(string flagName)
    {
        return !IsEnabled(flagName);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, bool> GetAll()
    {
        return configurationAccessor.Current.Flags;
    }
}

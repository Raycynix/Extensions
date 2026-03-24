using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Configurations;

namespace Raycynix.Extensions.Configuration.Implementations;

internal sealed class FeatureFlagAccessor(
    IConfigurationAccessor<FeatureFlagsConfiguration> configurationAccessor) : IFeatureFlagAccessor
{
    public bool IsEnabled(string flagName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(flagName);

        return configurationAccessor.Current.Flags.TryGetValue(flagName, out var enabled) && enabled;
    }

    public bool IsDisabled(string flagName)
    {
        return !IsEnabled(flagName);
    }

    public IReadOnlyDictionary<string, bool> GetAll()
    {
        return configurationAccessor.Current.Flags;
    }
}

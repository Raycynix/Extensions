using Microsoft.Extensions.Options;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Internal;

namespace Raycynix.Extensions.Configuration.Implementations;

/// <summary>
/// Provides typed access to the current approved configuration snapshot.
/// </summary>
internal sealed class ConfigurationAccessor<TOptions>(
    IOptionsMonitor<TOptions> optionsMonitor,
    ConfigurationRuntimeState<TOptions> runtimeState)
    : IConfigurationAccessor<TOptions>
    where TOptions : class
{
    /// <inheritdoc />
    public TOptions Current => runtimeState.Current ?? optionsMonitor.CurrentValue;

    /// <inheritdoc />
    public TOptions Get(string? name)
    {
        return string.IsNullOrWhiteSpace(name)
            ? Current
            : optionsMonitor.Get(name);
    }
}

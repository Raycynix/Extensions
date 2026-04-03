using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;

namespace Raycynix.Extensions.Configuration.Internal;

/// <summary>
/// Default reload policy that allows all runtime configuration changes.
/// </summary>
internal sealed class AllowConfigurationReloadPolicy<TOptions> : IConfigurationReloadPolicy<TOptions>
    where TOptions : class
{
    /// <inheritdoc />
    public ConfigurationReloadResult Evaluate(ConfigurationChangeContext<TOptions> context)
    {
        return ConfigurationReloadResult.Apply();
    }
}

using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;

namespace Raycynix.Extensions.Configuration.Internal;

internal sealed class AllowConfigurationReloadPolicy<TOptions> : IConfigurationReloadPolicy<TOptions>
    where TOptions : class
{
    public ConfigurationReloadResult Evaluate(ConfigurationChangeContext<TOptions> context)
    {
        return ConfigurationReloadResult.Apply();
    }
}

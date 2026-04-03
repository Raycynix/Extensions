using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;

namespace Raycynix.Extensions.Configuration.Internal;

/// <summary>
/// Adapts an inline delegate to <see cref="IConfigurationReloadPolicy{TOptions}"/>.
/// </summary>
internal sealed class DelegateConfigurationReloadPolicy<TOptions>(
    Func<ConfigurationChangeContext<TOptions>, ConfigurationReloadResult> evaluate)
    : IConfigurationReloadPolicy<TOptions>
    where TOptions : class
{
    /// <inheritdoc />
    public ConfigurationReloadResult Evaluate(ConfigurationChangeContext<TOptions> context)
    {
        return evaluate(context);
    }
}

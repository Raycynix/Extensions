using Raycynix.Extensions.Configuration.Abstractions.Models;

namespace Raycynix.Extensions.Configuration.Abstractions.Interfaces;

/// <summary>
/// Evaluates whether a runtime configuration change may be applied.
/// </summary>
/// <typeparam name="TOptions">The configuration model type.</typeparam>
public interface IConfigurationReloadPolicy<TOptions>
    where TOptions : class
{
    /// <summary>
    /// Evaluates a configuration change before it is propagated to registered handlers.
    /// </summary>
    /// <param name="context">The configuration change context.</param>
    /// <returns>The reload evaluation result.</returns>
    ConfigurationReloadResult Evaluate(ConfigurationChangeContext<TOptions> context);
}

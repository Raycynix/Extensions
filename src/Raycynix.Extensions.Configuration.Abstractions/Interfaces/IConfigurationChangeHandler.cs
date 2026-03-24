using Raycynix.Extensions.Configuration.Abstractions.Models;

namespace Raycynix.Extensions.Configuration.Abstractions.Interfaces;

/// <summary>
/// Handles changes to a typed configuration model produced by the standard options monitor pipeline.
/// </summary>
/// <typeparam name="TOptions">The configuration model type.</typeparam>
public interface IConfigurationChangeHandler<TOptions>
    where TOptions : class
{
    /// <summary>
    /// Handles a configuration change notification.
    /// </summary>
    /// <param name="context">The change context containing previous and current values.</param>
    /// <param name="cancellationToken">The cancellation token for the current operation.</param>
    /// <returns>A task that completes when the change has been handled.</returns>
    ValueTask HandleAsync(
        ConfigurationChangeContext<TOptions> context,
        CancellationToken cancellationToken = default);
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;

namespace Raycynix.Extensions.Metrics;

/// <summary>
/// Provides service registration extensions for the metrics package.
/// </summary>
public static class Metrics
{
    /// <param name="services">The service collection to update.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the standard .NET metrics services and application <c>IMeterFactory</c>.
        /// </summary>
        /// <param name="configure">An optional callback for configuring the .NET metrics pipeline.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public IServiceCollection AddRaycynixMetrics(Action<IMetricsBuilder>? configure = null)
        {
            ArgumentNullException.ThrowIfNull(services);

            if (configure is null)
            {
                services.AddMetrics();
            }
            else
            {
                services.AddMetrics(configure);
            }

            return services;
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Metrics.Abstractions.Interfaces;
using Raycynix.Extensions.Metrics.Configurations;
using Raycynix.Extensions.Metrics.Implementations;
using Raycynix.Extensions.Metrics.Internal;

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
        /// Registers the metrics service together with the typed metrics configuration model.
        /// </summary>
        /// <param name="configuration">The application configuration source.</param>
        /// <param name="setup">An optional callback for adjusting the bound metrics configuration.</param>
        /// <param name="healthSetup">An optional callback for configuring health checks.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public IServiceCollection AddRaycynixMetrics(Microsoft.Extensions.Configuration.IConfiguration configuration,
            Action<MetricsConfiguration>? setup = null,
            Action<IHealthChecksBuilder>? healthSetup = null)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services.AddRaycynixConfiguration<MetricsConfiguration>(configuration, configurePostBind: setup);
            services.AddRaycynixConfigurationValidator<MetricsConfiguration, MetricsConfigurationValidator>();
            services.AddSingleton(serviceProvider =>
                serviceProvider.GetRequiredService<IConfigurationAccessor<MetricsConfiguration>>().Current);

            return services.AddRaycynixMetrics(healthSetup);
        }

        /// <summary>
        /// Registers the metrics service and optional health checks.
        /// </summary>
        /// <param name="healthSetup">An optional callback for configuring health checks.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public IServiceCollection AddRaycynixMetrics(Action<IHealthChecksBuilder>? healthSetup = null)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddSingleton<IMetricsService, MetricsService>();

            var healthBuilder = services.AddHealthChecks();
            healthSetup?.Invoke(healthBuilder);

            return services;
        }
    }
}
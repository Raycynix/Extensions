using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Metrics.Abstractions;
using Raycynix.Extensions.Metrics.Implementations;

namespace Raycynix.Extensions.Metrics;

/// <summary>
/// Provides extension methods for setting up metrics services within a dependency injection container.
/// </summary>
public static class Metrics
{
    /// <summary>
    /// Adds Raycynix Metrics services to the dependency injection container.
    /// Optionally, allows for further configuration of health checks during the setup.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> instance to which the metrics services will be added.
    /// </param>
    /// <param name="healthSetup">
    /// An optional action to configure the <see cref="IHealthChecksBuilder"/> during health checks setup.
    /// If not provided, no additional health checks configuration will be applied.
    /// </param>
    public static IServiceCollection AddRaycynixMetrics(this IServiceCollection services,
        Action<IHealthChecksBuilder>? healthSetup = null)
    {
        services.TryAddSingleton<IMetricsService, MetricsService>();

        var healthBuilder = services.AddHealthChecks();
        healthSetup?.Invoke(healthBuilder);
        
        return services;
    }
}
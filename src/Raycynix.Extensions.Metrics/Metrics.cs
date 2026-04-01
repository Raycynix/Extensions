using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Metrics.Abstractions;
using Raycynix.Extensions.Metrics.Abstractions.Interfaces;
using Raycynix.Extensions.Metrics.Implementations;

namespace Raycynix.Extensions.Metrics;

/// <summary>
/// Provides service registration extensions for the metrics package.
/// </summary>
public static class Metrics
{
    /// <summary>
    /// Registers the metrics service and optional health checks.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="healthSetup">An optional callback for configuring health checks.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixMetrics(this IServiceCollection services,
        Action<IHealthChecksBuilder>? healthSetup = null)
    {
        services.TryAddSingleton<IMetricsService, MetricsService>();

        var healthBuilder = services.AddHealthChecks();
        healthSetup?.Invoke(healthBuilder);
        
        return services;
    }
}

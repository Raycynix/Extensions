using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Metrics.Abstractions;

namespace Raycynix.Extensions.Metrics;

public static class Metrics
{
    public static IServiceCollection AddRaycynixMetrics(this IServiceCollection services,
        Action<IHealthChecksBuilder>? healthSetup = null)
    {
        services.TryAddSingleton<IMetricsService, IMetricsService>();

        var healthBuilder = services.AddHealthChecks();
        healthSetup?.Invoke(healthBuilder);

        return services;
    }
}
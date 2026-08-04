using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using Raycynix.Extensions.Metrics.Abstractions;

namespace Raycynix.Extensions.Metrics.AspNetCore;

/// <summary>
/// Provides OpenTelemetry metrics integration for ASP.NET Core applications.
/// </summary>
public static class Metrics
{
    /// <summary>
    /// Registers the Raycynix meter and ASP.NET Core request instrumentation with OpenTelemetry.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="configure">An optional callback for adding exporters or additional instrumentation.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixAspNetCoreMetrics(
        this IServiceCollection services,
        Action<MeterProviderBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddRaycynixMetrics();
        services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(RaycynixMetrics.MeterName)
                    .AddAspNetCoreInstrumentation();

                configure?.Invoke(metrics);
            });

        return services;
    }

}

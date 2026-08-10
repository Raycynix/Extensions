using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using Raycynix.Extensions.Tracing.Abstractions;

namespace Raycynix.Extensions.Tracing.AspNetCore;

/// <summary>
/// Provides OpenTelemetry tracing integration for ASP.NET Core applications.
/// </summary>
public static class Tracing
{
    /// <summary>
    /// Registers the Raycynix activity source and ASP.NET Core request instrumentation with OpenTelemetry.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="configure">An optional callback for adding exporters or additional instrumentation.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixAspNetCoreTracing(
        this IServiceCollection services,
        Action<TracerProviderBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddRaycynixTracing();
        services.Configure<LoggerFactoryOptions>(options =>
            options.ActivityTrackingOptions |= ActivityTrackingOptions.TraceId | ActivityTrackingOptions.SpanId);
        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(RaycynixTracing.SourceName)
                    .AddAspNetCoreInstrumentation();

                configure?.Invoke(tracing);
            });

        return services;
    }
}

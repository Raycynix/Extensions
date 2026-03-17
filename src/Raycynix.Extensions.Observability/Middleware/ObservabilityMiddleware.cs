using Microsoft.AspNetCore.Builder;
using Prometheus;
using Raycynix.Extensions.Tracing.Middleware;

namespace Raycynix.Extensions.Observability.Middleware;

/// <summary>
/// Provides extension methods for adding Raycynix observability middleware
/// components to the ASP.NET Core request pipeline.
/// </summary>
public static class ObservabilityMiddleware
{
    /// <summary>
    /// Adds middleware that enriches logs with tracing data, propagates
    /// correlation identifiers, and records HTTP request metrics.
    /// </summary>
    /// <param name="app">The application builder used to configure the request pipeline.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> instance for chaining.</returns>
    public static IApplicationBuilder UseRaycynixObservability(this IApplicationBuilder app)
    {
        app.UseMiddleware<TracingMiddleware>();
        app.UseMiddleware<CorrelationMiddleware>();
        app.UseHttpMetrics();

        return app;
    }
}

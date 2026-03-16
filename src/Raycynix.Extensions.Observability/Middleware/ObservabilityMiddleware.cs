using Microsoft.AspNetCore.Builder;
using Prometheus;
using Raycynix.Extensions.Tracing.Middleware;

namespace Raycynix.Extensions.Observability.Middleware;

/// <summary>
/// Provides middleware extension methods to integrate observability features
/// into the application's request pipeline.
/// </summary>
public static class ObservabilityMiddleware
{
    /// <summary>
    /// Configures the application pipeline to include observability features
    /// such as tracing, correlation, HTTP request metrics, health checks,
    /// and Prometheus metrics endpoints.
    /// </summary>
    /// <param name="app">The application builder used to configure the request pipeline.</param>
    /// <returns>The modified application builder to support observability features.</returns>
    public static IApplicationBuilder UseRaycynixObservability(this IApplicationBuilder app)
    {
        app.UseMiddleware<TracingMiddleware>();
        app.UseMiddleware<CorrelationMiddleware>();
        app.UseHttpMetrics();

        return app;
    }
}
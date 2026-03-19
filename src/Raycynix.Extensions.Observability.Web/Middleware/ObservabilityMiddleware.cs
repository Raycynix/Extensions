using Microsoft.AspNetCore.Builder;
using Raycynix.Extensions.Metrics.Web;
using Raycynix.Extensions.Tracing.Web;
using Raycynix.Extensions.Observability.Web.Middleware;

namespace Raycynix.Extensions.Observability.Web.Middleware;

/// <summary>
/// Provides middleware extensions for adding Raycynix observability components to the ASP.NET Core request pipeline.
/// </summary>
public static class ObservabilityMiddleware
{
    /// <summary>
    /// Adds tracing, correlation, and HTTP metrics middleware to the request pipeline.
    /// </summary>
    /// <param name="app">The application builder used to configure the request pipeline.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> instance for chaining.</returns>
    public static IApplicationBuilder UseRaycynixObservability(this IApplicationBuilder app)
    {
        app.UseRaycynixTracing();
        app.UseMiddleware<CorrelationMiddleware>();
        app.UseRaycynixMetrics();

        return app;
    }
}

using Microsoft.AspNetCore.Builder;
using Raycynix.Extensions.Tracing.AspNetCore;

namespace Raycynix.Extensions.Observability.AspNetCore.Middleware;

/// <summary>
/// Provides middleware extensions for adding Raycynix observability components to the ASP.NET Core request pipeline.
/// </summary>
public static class ObservabilityMiddleware
{
    /// <summary>
    /// Adds tracing and correlation middleware to the request pipeline.
    /// ASP.NET Core metrics are collected through the OpenTelemetry instrumentation registered by
    /// <c>AddRaycynixAspNetCoreObservability</c> and require no request middleware.
    /// </summary>
    /// <param name="app">The application builder used to configure the request pipeline.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> instance for chaining.</returns>
    public static IApplicationBuilder UseRaycynixObservability(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseRaycynixTracing();
        app.UseMiddleware<CorrelationMiddleware>();
        return app;
    }
}

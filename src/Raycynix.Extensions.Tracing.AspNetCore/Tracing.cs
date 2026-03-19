using Microsoft.AspNetCore.Builder;
using Raycynix.Extensions.Tracing.AspNetCore.Middleware;

namespace Raycynix.Extensions.Tracing.AspNetCore;

/// <summary>
/// Provides middleware extensions for the Raycynix tracing package.
/// </summary>
public static class Tracing
{
    /// <summary>
    /// Adds the Raycynix tracing middleware to the ASP.NET Core request pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The configured application builder.</returns>
    public static IApplicationBuilder UseRaycynixTracing(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TracingMiddleware>();
    }
}

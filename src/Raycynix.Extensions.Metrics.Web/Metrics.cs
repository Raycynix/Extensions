using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Prometheus;

namespace Raycynix.Extensions.Metrics.Web;

/// <summary>
/// Provides ASP.NET Core integration extensions for the Raycynix metrics package.
/// </summary>
public static class Metrics
{
    /// <summary>
    /// Adds HTTP request metrics collection to the ASP.NET Core request pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The configured application builder.</returns>
    public static IApplicationBuilder UseRaycynixMetrics(this IApplicationBuilder app)
    {
        return app.UseHttpMetrics();
    }

    /// <summary>
    /// Maps the Prometheus metrics endpoint.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="path">The endpoint path for metrics output.</param>
    /// <returns>The same <see cref="IEndpointRouteBuilder"/> instance for chaining.</returns>
    public static IEndpointRouteBuilder MapRaycynixMetrics(
        this IEndpointRouteBuilder endpoints,
        string path = "/metrics")
    {
        endpoints.MapMetrics(path);

        return endpoints;
    }
}

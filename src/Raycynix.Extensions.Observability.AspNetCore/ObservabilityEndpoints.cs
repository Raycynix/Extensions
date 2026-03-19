using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Raycynix.Extensions.Metrics.AspNetCore;

namespace Raycynix.Extensions.Observability.AspNetCore;

/// <summary>
/// Provides endpoint mapping extensions for Raycynix observability endpoints.
/// </summary>
public static class ObservabilityEndpoints
{
    /// <summary>
    /// Maps health check and metrics endpoints.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="healthPath">The health checks endpoint path.</param>
    /// <param name="metricsPath">The metrics endpoint path.</param>
    /// <returns>The same <see cref="IEndpointRouteBuilder"/> instance for chaining.</returns>
    public static IEndpointRouteBuilder MapRaycynixObservabilityEndpoints(
        this IEndpointRouteBuilder endpoints,
        string healthPath = "/health",
        string metricsPath = "/metrics")
    {
        endpoints.MapHealthChecks(healthPath);
        endpoints.MapRaycynixMetrics(metricsPath);

        return endpoints;
    }
}

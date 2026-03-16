using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Prometheus;

namespace Raycynix.Extensions.Observability;

/// <summary>
/// Provides endpoint mapping extensions for Raycynix observability features.
/// </summary>
public static class ObservabilityEndpoints
{
    /// <summary>
    /// Maps observability endpoints such as health checks and Prometheus metrics.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="healthPath">The health checks endpoint path.</param>
    /// <param name="metricsPath">The metrics endpoint path.</param>
    /// <returns>The endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapRaycynixObservabilityEndpoints(
        this IEndpointRouteBuilder endpoints,
        string healthPath = "/health",
        string metricsPath = "/metrics")
    {
        endpoints.MapHealthChecks(healthPath);
        endpoints.MapMetrics(metricsPath);

        return endpoints;
    }
}
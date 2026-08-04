using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Raycynix.Extensions.Observability.AspNetCore;

/// <summary>
/// Provides endpoint mapping extensions for Raycynix observability endpoints.
/// </summary>
public static class ObservabilityEndpoints
{
    /// <summary>
    /// Maps the health check endpoint. Metrics exporters expose their own endpoints explicitly.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="healthPath">The health checks endpoint path.</param>
    /// <returns>The same <see cref="IEndpointRouteBuilder"/> instance for chaining.</returns>
    public static IEndpointRouteBuilder MapRaycynixObservabilityEndpoints(
        this IEndpointRouteBuilder endpoints,
        string healthPath = "/health")
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapHealthChecks(healthPath);
        return endpoints;
    }
}

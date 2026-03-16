using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Common.Context;
using Raycynix.Extensions.Logging;
using Raycynix.Extensions.Metrics;
using Raycynix.Extensions.Observability.Http;
using Raycynix.Extensions.Tracing;

namespace Raycynix.Extensions.Observability;

/// <summary>
/// Provides methods for configuring and adding observability features to an application,
/// including metrics, tracing, and logging capabilities.
/// </summary>
public static class Observability
{
    /// <summary>
    /// Adds Raycynix observability features to the service collection, including capabilities
    /// for metrics, tracing, and logging. Also sets up operation context and HTTP handlers
    /// for correlation support.
    /// </summary>
    /// <param name="services">The IServiceCollection to which the observability features will be added.</param>
    /// <returns>The updated IServiceCollection with Raycynix observability services registered.</returns>
    public static IServiceCollection AddRaycynixObservability(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddRaycynixMetrics();
        services.AddRaycynixTracing();
        services.AddRaycynixLogging();

        services.TryAddScoped<IOperationContext, OperationContext>();

        services.AddTransient<CorrelationHeaderHandler>();

        return services;
    }
}
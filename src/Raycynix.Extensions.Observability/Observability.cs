using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Common.Context;
using Raycynix.Extensions.Logging;
using Raycynix.Extensions.Metrics;
using Raycynix.Extensions.Tracing;

namespace Raycynix.Extensions.Observability;

/// <summary>
/// Provides service registration extensions for the observability package.
/// </summary>
public static class Observability
{
    /// <summary>
    /// Registers metrics, tracing, logging, and the ambient operation context.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixObservability(this IServiceCollection services)
    {
        services.AddRaycynixMetrics();
        services.AddRaycynixTracing();
        services.AddRaycynixLogging();

        services.TryAddScoped<IOperationContext, OperationContext>();

        return services;
    }
}

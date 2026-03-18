using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Raycynix.Extensions.Common.Context;
using Raycynix.Extensions.Logging;
using Raycynix.Extensions.Metrics;
using Raycynix.Extensions.Observability.Http;
using Raycynix.Extensions.Tracing;

namespace Raycynix.Extensions.Observability;

/// <summary>
/// Provides service registration extensions for the observability package.
/// </summary>
public static class Observability
{
    /// <summary>
    /// Registers metrics, tracing, logging, operation context, and correlation propagation.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixObservability(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddRaycynixMetrics();
        services.AddRaycynixTracing();
        services.AddRaycynixLogging();

        services.TryAddScoped<IOperationContext, OperationContext>();

        services.AddTransient<CorrelationHeaderHandler>();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, CorrelationHttpMessageHandlerBuilderFilter>());

        return services;
    }
}

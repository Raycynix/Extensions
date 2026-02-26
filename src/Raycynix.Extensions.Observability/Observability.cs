using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Common.Context;
using Raycynix.Extensions.Metrics;
using Raycynix.Extensions.Observability.Http;
using Raycynix.Extensions.Tracing;
using Raycynix.Extensions.Logging;

namespace Raycynix.Extensions.Observability;

public static class Observability
{
    public static IServiceCollection AddRaycynixObservability(this IServiceCollection services)
    {
        services.AddRaycynixMetrics();
        services.AddRaycynixTracing();
        services.AddRaycynixLogging();
        
        services.TryAddScoped<IOperationContext, OperationContext>();
        
        services.AddTransient<CorrelationHeaderHandler>();
        
        return services;
    }
}
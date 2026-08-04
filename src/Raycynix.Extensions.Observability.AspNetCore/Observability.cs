using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Raycynix.Extensions.Observability.AspNetCore.Configurations;
using Raycynix.Extensions.Observability.AspNetCore.Http;
using Raycynix.Extensions.Metrics.AspNetCore;

namespace Raycynix.Extensions.Observability.AspNetCore;

/// <summary>
/// Provides ASP.NET Core service registration extensions for the observability package.
/// </summary>
public static class Observability
{
    /// <summary>
    /// Registers the core observability services and ASP.NET Core-specific integrations such as correlation propagation.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="setup">An optional callback for adjusting ASP.NET Core observability options.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixAspNetCoreObservability(
        this IServiceCollection services,
        Action<ObservabilityAspNetCoreConfiguration>? setup = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddRaycynixObservability();
        services.AddRaycynixAspNetCoreMetrics();
        services.AddHealthChecks();
        services.Configure<ObservabilityAspNetCoreConfiguration>(options => setup?.Invoke(options));
        
        services.AddHttpContextAccessor();

        services.AddTransient<CorrelationHeaderHandler>();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, CorrelationHttpMessageHandlerBuilderFilter>());

        return services;
    }
}

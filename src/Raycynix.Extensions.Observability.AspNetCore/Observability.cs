using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Raycynix.Extensions.Observability.AspNetCore.Http;

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
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixAspNetCoreObservability(this IServiceCollection services)
    {
        services.AddRaycynixObservability();
        
        services.AddHttpContextAccessor();

        services.AddTransient<CorrelationHeaderHandler>();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, CorrelationHttpMessageHandlerBuilderFilter>());

        return services;
    }
}

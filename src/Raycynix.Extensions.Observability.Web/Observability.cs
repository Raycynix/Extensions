using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Raycynix.Extensions.Observability.Web.Http;

namespace Raycynix.Extensions.Observability.Web;

/// <summary>
/// Provides ASP.NET Core service registration extensions for the observability package.
/// </summary>
public static class Observability
{
    /// <summary>
    /// Registers ASP.NET Core-specific observability services such as correlation propagation.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixWebObservability(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddTransient<CorrelationHeaderHandler>();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, CorrelationHttpMessageHandlerBuilderFilter>());

        return services;
    }
}

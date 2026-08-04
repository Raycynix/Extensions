using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Tracing.Abstractions;

namespace Raycynix.Extensions.Tracing;

/// <summary>
/// Provides service registration extensions for the tracing package.
/// </summary>
public static class Tracing
{
    /// <summary>
    /// Registers the shared standard .NET activity source.
    /// </summary>
    /// <param name="services">The service collection to which the tracing services are to be added.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixTracing(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (!services.Any(descriptor =>
                descriptor.ServiceType == typeof(ActivitySource) &&
                ReferenceEquals(descriptor.ImplementationInstance, RaycynixTracing.ActivitySource)))
        {
            services.AddSingleton(RaycynixTracing.ActivitySource);
        }

        return services;
    }
}

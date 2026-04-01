using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Common.Helpers;
using Raycynix.Extensions.Tracing.Abstractions;
using Raycynix.Extensions.Tracing.Abstractions.Interfaces;
using Raycynix.Extensions.Tracing.Implementations;

namespace Raycynix.Extensions.Tracing;

/// <summary>
/// Provides service registration extensions for the tracing package.
/// </summary>
public static class Tracing
{
    /// <summary>
    /// Registers the tracing service.
    /// </summary>
    /// <param name="services">The service collection to which the tracing services are to be added.</param>
    public static IServiceCollection AddRaycynixTracing(this IServiceCollection services)
    {
        var serviceName = AssemblyHelper.CurrentName();

        services.TryAddSingleton<ITracer>(new Tracer(serviceName));

        return services;
    }
}

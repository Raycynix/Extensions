using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Common.Helpers;
using Raycynix.Extensions.Tracing.Abstractions;
using Raycynix.Extensions.Tracing.Implementation;

namespace Raycynix.Extensions.Tracing;

/// <summary>
/// Provides extension methods to configure and enable tracing functionality using the Raycynix Tracing library.
/// </summary>
public static class Tracing
{
    /// <summary>
    /// Adds tracing services to the dependency injection container for the application.
    /// </summary>
    /// <param name="services">The service collection to which the tracing services are to be added.</param>
    public static void AddRaycynixTracing(this IServiceCollection services)
    {
        var serviceName = AssemblyHelper.CurrentName();
        
        services.TryAddSingleton<ITracer>(new Tracer(serviceName));
    }
}
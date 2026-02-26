using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Common.Helpers;
using Raycynix.Extensions.Tracing.Abstractions;
using Raycynix.Extensions.Tracing.Implementation;

namespace Raycynix.Extensions.Tracing;

public static class Tracing
{
    public static void AddRaycynixTracing(this IServiceCollection services)
    {
        var serviceName = AssemblyHelper.CurrentName();
        
        services.TryAddSingleton<ITracer>(new Tracer(serviceName));
    }
}
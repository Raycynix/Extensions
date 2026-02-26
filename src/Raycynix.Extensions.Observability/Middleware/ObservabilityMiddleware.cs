using Microsoft.AspNetCore.Builder;
using Prometheus;
using Raycynix.Extensions.Tracing.Middleware;

namespace Raycynix.Extensions.Observability.Middleware;

public static class ObservabilityMiddleware
{
    public static IApplicationBuilder UseRaycynixObservability(this IApplicationBuilder app)
    {
        app.UseMiddleware<TracingMiddleware>();
        app.UseMiddleware<CorrelationMiddleware>();
        
        app.UseHttpMetrics();
        
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapMetrics();

            endpoints.MapHealthChecks("/health");
        });

        return app;
    }
}
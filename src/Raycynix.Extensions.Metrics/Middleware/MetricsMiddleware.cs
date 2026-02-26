using Microsoft.AspNetCore.Builder;
using Prometheus;

namespace Raycynix.Extensions.Metrics.Middleware;

public static class MetricsMiddleware
{
    public static IApplicationBuilder UseRaycynixMetrics(this IApplicationBuilder app)
    {
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
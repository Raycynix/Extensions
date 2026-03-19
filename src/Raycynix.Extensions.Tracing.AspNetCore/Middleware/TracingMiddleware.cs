using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Raycynix.Extensions.Tracing.AspNetCore.Middleware;

/// <summary>
/// Middleware that enriches the Serilog logging context with trace identifiers
/// resolved from the current diagnostic activity.
/// </summary>
public class TracingMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Adds <c>TraceId</c> and <c>SpanId</c> values to the logging context for the
    /// current request and then passes execution to the next middleware component.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task that completes when the remaining pipeline has finished processing.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var activity = Activity.Current;
        var traceId = activity?.TraceId.ToString() ?? context.TraceIdentifier;
        var spanId = activity?.SpanId.ToString();

        using (LogContext.PushProperty("TraceId", traceId))
        using (LogContext.PushProperty("SpanId", spanId))
        {
            await next(context);
        }
    }
}

using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Raycynix.Extensions.Tracing.Middleware;

/// <summary>
/// Middleware that enriches logs with trace information from the current activity.
/// </summary>
public class TracingMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Processes an incoming HTTP request by ensuring a correlation ID is present in the headers.
    /// If a correlation ID is not provided, a new one is generated. The correlation ID is added
    /// to the response headers and attached to the logging context.
    /// </summary>
    /// <param name="context">The HTTP context representing the current request.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation of the middleware.</returns>
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
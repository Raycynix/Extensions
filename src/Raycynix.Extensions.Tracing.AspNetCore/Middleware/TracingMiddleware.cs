using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Raycynix.Extensions.Tracing.AspNetCore.Middleware;

/// <summary>
/// Middleware that exposes trace identifiers through a standard logging scope
/// resolved from the current diagnostic activity.
/// </summary>
public class TracingMiddleware(RequestDelegate next, ILoggerFactory? loggerFactory = null)
{
    /// <summary>
    /// Adds <c>TraceId</c> and <c>SpanId</c> values to the logging scope for the
    /// current request and then passes execution to the next middleware component.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task that completes when the remaining pipeline has finished processing.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var activity = Activity.Current;
        var traceId = activity?.TraceId.ToString() ?? context.TraceIdentifier;
        var spanId = activity?.SpanId.ToString();

        var logger = loggerFactory?.CreateLogger<TracingMiddleware>();
        using (logger?.BeginScope(new Dictionary<string, object?>
               {
                   ["TraceId"] = traceId,
                   ["SpanId"] = spanId
               }))
        {
            logger?.LogDebug(
                "Resolved request trace context. HasActivity:{HasActivity} HasSpanId:{HasSpanId}",
                activity is not null,
                !string.IsNullOrWhiteSpace(spanId));

            await next(context);
        }
    }
}
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Raycynix.Extensions.Tracing.Middleware;

/// <summary>
/// Middleware that handles tracing and correlation for HTTP requests by ensuring each request
/// contains a unique correlation identifier. It propagates this identifier in the response
/// headers and enriches log entries for better traceability.
/// </summary>
public class TracingMiddleware(RequestDelegate next)
{
    private const string CorrelationHeader = "X-Correlation-Id";

    /// <summary>
    /// Processes an incoming HTTP request by ensuring a correlation ID is present in the headers.
    /// If a correlation ID is not provided, a new one is generated. The correlation ID is added
    /// to the response headers and attached to the logging context.
    /// </summary>
    /// <param name="context">The HTTP context representing the current request.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation of the middleware.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(CorrelationHeader, out var correlationId))
        {
            correlationId = Activity.Current?.RootId ?? Guid.NewGuid().ToString();
        }

        context.Response.Headers[CorrelationHeader] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}
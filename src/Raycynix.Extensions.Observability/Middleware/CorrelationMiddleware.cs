using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Raycynix.Extensions.Common.Context;
using Raycynix.Extensions.Observability.Http;
using Serilog.Context;

namespace Raycynix.Extensions.Observability.Middleware;

/// <summary>
/// Middleware that manages correlation and trace identifiers for HTTP requests,
/// ensuring consistent tracking and logging across the application's request/response pipeline.
/// </summary>
public class CorrelationMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Handles the incoming HTTP request, extracts or assigns correlation and trace identifiers,
    /// and enriches the logging context to include relevant metadata.
    /// </summary>
    /// <param name="context">The current HTTP context of the request.</param>
    /// <param name="operationContext">The operation context containing correlation, trace, and user identifiers.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context, IOperationContext operationContext)
    {
        var correlationId = context.Request.Headers.TryGetValue(CorrelationHeaderHandler.CorrelationHeader, out var headerValue)
            ? headerValue.ToString()
            : context.TraceIdentifier;

        operationContext.SetCorrelationIdIfMissing(correlationId);
        context.Response.Headers[CorrelationHeaderHandler.CorrelationHeader] = operationContext.CorrelationId;

        var userId = context.User?.Identity?.Name;
        if (!string.IsNullOrWhiteSpace(userId))
        {
            operationContext.UserId = userId;
        }

        var activity = Activity.Current;
        var traceId = activity?.TraceId.ToString() ?? operationContext.TraceId;

        activity?.SetTag("correlation.id", operationContext.CorrelationId);
        activity?.SetTag("trace.id", traceId);

        if (!string.IsNullOrWhiteSpace(operationContext.UserId))
        {
            activity?.SetTag("user.id", operationContext.UserId);
        }

        using (LogContext.PushProperty("CorrelationId", operationContext.CorrelationId))
        using (LogContext.PushProperty("TraceId", traceId))
        using (LogContext.PushProperty("UserId", operationContext.UserId))
        {
            await next(context);
        }
    }
}
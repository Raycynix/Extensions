using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Raycynix.Extensions.Common.Context;
using Raycynix.Extensions.Observability.AspNetCore.Http;
using Serilog.Context;

namespace Raycynix.Extensions.Observability.AspNetCore.Middleware;

/// <summary>
/// Resolves correlation data for the current request and enriches logs and tracing context.
/// </summary>
public class CorrelationMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Adds correlation, trace, and user identifiers to the current request context.
    /// </summary>
    /// <param name="context">The current HTTP context of the request.</param>
    /// <param name="operationContext">The operation context containing correlation, trace, and user identifiers.</param>
    /// <returns>A task that completes when the remaining pipeline has finished processing.</returns>
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
            OperationContext.Current = operationContext;

            try
            {
                await next(context);
            }
            finally
            {
                OperationContext.Current = null;
            }
        }
    }
}

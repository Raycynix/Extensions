using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Raycynix.Extensions.Common.Context;
using Serilog.Context;

namespace Raycynix.Extensions.Logging.Middleware;

//TODO: Create Documentation

/// <summary>
/// 
/// </summary>
/// <param name="next"></param>
public class CorrelationMiddleware(RequestDelegate next)
{
    private const string CorrelationHeader = "X-Correlation-ID";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="operationContext"></param>
    public async Task InvokeAsync(HttpContext context, IOperationContext operationContext)
    {
        if (!context.Request.Headers.TryGetValue(CorrelationHeader, out var correlationId))
        {
            correlationId = operationContext.CorrelationId;
        }

        operationContext.CorrelationId = correlationId!;
        context.Response.Headers[CorrelationHeader] = correlationId;

        var userId = context.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(userId))
        {
            operationContext.UserId = userId;
        }

        var traceId = Activity.Current?.TraceId.ToString() ?? operationContext.TraceId;
        var activity = Activity.Current;
        activity?.SetTag("correlation.id", correlationId);
        activity?.SetTag("trace.id", traceId);
        activity?.SetTag("user.id", userId);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("TraceId", traceId))
        using (LogContext.PushProperty("UserId", operationContext.UserId))
        {
            await next(context);
        }
    }
}
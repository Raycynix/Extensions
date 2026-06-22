using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Common.Context;
using Raycynix.Extensions.Observability.AspNetCore.Http;
using Raycynix.Extensions.Security.Abstractions.Enums;
using Raycynix.Extensions.Security.Abstractions.Interfaces;

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
    /// <param name="operationContext">The operation context containing correlation, trace, and subject identifiers.</param>
    /// <param name="serviceProvider">The request service provider used to resolve optional integrations.</param>
    /// <returns>A task that completes when the remaining pipeline has finished processing.</returns>
    public async Task InvokeAsync(
        HttpContext context,
        IOperationContext operationContext,
        IServiceProvider serviceProvider)
    {
        var securityContext = serviceProvider.GetService<ISecurityContext>();
        var correlationId =
            context.Request.Headers.TryGetValue(CorrelationHeaderHandler.CorrelationHeader, out var headerValue)
                ? headerValue.ToString()
                : context.TraceIdentifier;

        operationContext.SetCorrelationIdIfMissing(correlationId);
        context.Response.Headers[CorrelationHeaderHandler.CorrelationHeader] = operationContext.CorrelationId;

        var userId = context.User.Identity?.Name;
        if (!string.IsNullOrWhiteSpace(userId))
        {
            operationContext.UserId = userId;
        }

        if (securityContext is not null && securityContext.IsAuthenticated)
        {
            operationContext.SubjectId = securityContext.SubjectId;
            operationContext.SubjectType = securityContext.SubjectType.ToString();

            if (securityContext.SubjectType == SecuritySubjectType.User &&
                string.IsNullOrWhiteSpace(operationContext.UserId))
            {
                operationContext.UserId = securityContext.SubjectId;
            }
        }

        var activity = Activity.Current;
        var traceId = activity?.TraceId.ToString() ?? operationContext.TraceId;

        activity?.SetTag("correlation.id", operationContext.CorrelationId);
        activity?.SetTag("trace.id", traceId);

        if (!string.IsNullOrWhiteSpace(operationContext.UserId))
        {
            activity?.SetTag("user.id", operationContext.UserId);
        }

        if (!string.IsNullOrWhiteSpace(operationContext.SubjectId))
        {
            activity?.SetTag("subject.id", operationContext.SubjectId);
        }

        if (!string.IsNullOrWhiteSpace(operationContext.SubjectType))
        {
            activity?.SetTag("subject.type", operationContext.SubjectType);
        }

        var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<CorrelationMiddleware>();
        using (logger?.BeginScope(new Dictionary<string, object?>
               {
                   ["CorrelationId"] = operationContext.CorrelationId,
                   ["TraceId"] = traceId
               }))
        {
            logger?.LogDebug(
                "Resolved request correlation context. HasUserId:{HasUserId} HasSubjectId:{HasSubjectId} SubjectType:{SubjectType}",
                !string.IsNullOrWhiteSpace(operationContext.UserId),
                !string.IsNullOrWhiteSpace(operationContext.SubjectId),
                operationContext.SubjectType);

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
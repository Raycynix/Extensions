using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Common.Context;
using Raycynix.Extensions.Exceptions.Abstractions;
using Raycynix.Extensions.Exceptions.Abstractions.Enums;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;
using Raycynix.Extensions.Exceptions.Defaults;

namespace Raycynix.Extensions.Exceptions.AspNetCore.Middleware;

/// <summary>
/// Converts unhandled exceptions into structured JSON error responses.
/// </summary>
public class RaycynixExceptionMiddleware(
    RequestDelegate next,
    IExceptionMapper mapper,
    IExceptionDataMasker masker,
    ILogger<RaycynixExceptionMiddleware> logger)
{
    private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Executes the next middleware and handles any exception that escapes the pipeline.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="operationContext">The operation context for the current request.</param>
    /// <returns>A task that completes when the request has been processed.</returns>
    public async Task InvokeAsync(HttpContext httpContext, IOperationContext operationContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (OperationCanceledException) when (httpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogWarning("Request was canceled by the client. TraceId: {TraceId}", httpContext.TraceIdentifier);
        }
        catch (Exception ex)
        {
            var raycynixException = mapper.Map(ex);
            var safeDetails = masker.Mask(raycynixException.SecureDetails);
            var traceId = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;
            var spanId = Activity.Current?.SpanId.ToString();
            var correlationId = operationContext.CorrelationId;
            var path = httpContext.Request.Path.Value;
            var method = httpContext.Request.Method;
            var endpoint = httpContext.GetEndpoint()?.DisplayName;
            var queryString = httpContext.Request.QueryString.HasValue
                ? httpContext.Request.QueryString.Value
                : null;
            var isTransient = raycynixException.Category == ErrorCategory.Transient;
            var retryAfterSeconds = raycynixException is TransientFailureException transientFailureException
                ? transientFailureException.RetryAfterSeconds
                : null;

            var executionContext = raycynixException.ExecutionContext ?? new ErrorExecutionContext(
                Source: "http",
                OperationName: endpoint,
                TraceId: traceId,
                SpanId: spanId,
                CorrelationId: correlationId,
                UserId: operationContext.UserId,
                Path: path,
                Method: method,
                Endpoint: endpoint,
                QueryString: queryString,
                IsTransient: isTransient);

            logger.LogError(ex,
                "Error {Code}: {Msg}. Category: {Category}. TraceId: {TraceId}. SpanId: {SpanId}. CorrelationId: {CorrelationId}. Method: {Method}. Path: {Path}. Endpoint: {Endpoint}. Query: {Query}. Details: {@Details}",
                raycynixException.ErrorCode,
                raycynixException.Message,
                raycynixException.Category,
                traceId,
                spanId,
                correlationId,
                method,
                path,
                endpoint,
                queryString,
                safeDetails);

            if (httpContext.Response.HasStarted)
            {
                logger.LogWarning(
                    "The response has already started, the error response middleware will not be executed. TraceId: {TraceId}",
                    traceId);

                throw;
            }

            var validationErrors = raycynixException is ValidationException validationException
                ? validationException.ValidationErrors
                : null;

            var response = new ExceptionResponse(
                Message: raycynixException.Message,
                ErrorCode: raycynixException.ErrorCode,
                Category: raycynixException.Category.ToString().ToLowerInvariant(),
                TraceId: traceId,
                SpanId: spanId,
                CorrelationId: correlationId,
                Path: path,
                Method: method,
                Endpoint: endpoint,
                QueryString: queryString,
                TimestampUtc: DateTimeOffset.UtcNow,
                Context: executionContext,
                IsTransient: isTransient,
                RetryAfterSeconds: retryAfterSeconds,
                Details: raycynixException.Details,
                ValidationErrors: validationErrors);

            httpContext.Response.Clear();
            httpContext.Response.StatusCode = raycynixException.StatusCode;
            httpContext.Response.ContentType = "application/json; charset=utf-8";
            if (retryAfterSeconds is not null)
            {
                httpContext.Response.Headers.RetryAfter = retryAfterSeconds.Value.ToString();
            }

            var json = JsonSerializer.Serialize(response, _serializerOptions);
            await httpContext.Response.WriteAsync(json);
        }
    }
}

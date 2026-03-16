using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Common.Context;
using Raycynix.Extensions.Exceptions.Abstractions;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;
using Raycynix.Extensions.Exceptions.Defaults;

namespace Raycynix.Extensions.Exceptions.Middleware;

/// <summary>
/// Middleware to handle exceptions during the HTTP request pipeline execution.
/// This middleware intercepts exceptions thrown by downstream components, maps them to a standardized
/// <see cref="RaycynixException"/>, masks sensitive data, logs the error, and generates a JSON response
/// containing the error details in a secure and consumable format.
/// </summary>
/// <remarks>
/// This middleware relies on the following dependencies:
/// - <see cref="IExceptionMapper"/>: For mapping exceptions to <see cref="RaycynixException"/>.
/// - <see cref="IExceptionDataMasker"/>: For masking sensitive data in exception details.
/// - <see cref="ILogger{T}"/>: For logging error details.
/// Exceptions are logged using the provided logger with an indication of the error code, message,
/// trace identifier, and masked details. The response generated includes an HTTP status code,
/// the mapped exception's error code, and a trace identifier for debugging purposes.
/// </remarks>
/// <param name="next">The next middleware delegate in the HTTP pipeline.</param>
/// <param name="mapper">The service used to map exceptions to <see cref="RaycynixException"/> objects.</param>
/// <param name="masker">The service used to mask sensitive data within exception details.</param>
/// <param name="logger">The logger instance used for logging error details.</param>
public class RaycynixExceptionMiddleware(
    RequestDelegate next,
    IExceptionMapper mapper,
    IExceptionDataMasker masker,
    ILogger<RaycynixExceptionMiddleware> logger)
{
    /// <summary>
    /// Processes an incoming HTTP request asynchronously, handling exceptions, mapping them
    /// to a custom exception type, masking secure details, and returning a standardized response
    /// in the event of an error.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> of the current HTTP request.</param>
    /// <returns>A task that represents the asynchronous execution of the middleware logic.</returns>
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
            var traceId = httpContext.TraceIdentifier;
            var correlationId = operationContext.CorrelationId;
            var path = httpContext.Request.Path.Value;

            logger.LogError(ex,
                "Error {Code}: {Msg}. TraceId: {TraceId}. CorrelationId: {CorrelationId}. Path: {Path}. Details: {@Details}",
                raycynixException.ErrorCode,
                raycynixException.Message,
                traceId,
                correlationId,
                path,
                safeDetails);

            if (httpContext.Response.HasStarted)
            {
                logger.LogWarning(
                    "The response has already started, the error response middleware will not be executed. TraceId: {TraceId}",
                    traceId);

                throw;
            }

            var response = new DefaultExceptionResponse(
                Message: raycynixException.Message,
                ErrorCode: raycynixException.ErrorCode,
                TraceId: traceId,
                CorrelationId: correlationId,
                Path: path,
                TimestampUtc: DateTimeOffset.UtcNow);

            httpContext.Response.Clear();
            httpContext.Response.StatusCode = raycynixException.StatusCode;
            httpContext.Response.ContentType = "application/json; charset=utf-8";

            var json = JsonSerializer.Serialize(response);
            await httpContext.Response.WriteAsync(json);
        }
    }
}
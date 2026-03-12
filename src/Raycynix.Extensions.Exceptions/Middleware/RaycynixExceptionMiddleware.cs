using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;
using Raycynix.Extensions.Exceptions.Defaults;

namespace Raycynix.Extensions.Exceptions.Middleware;

public class RaycynixExceptionMiddleware(
    RequestDelegate next,
    IExceptionMapper mapper,
    IExceptionDataMasker masker,
    ILogger<RaycynixExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            var raycynixException = mapper.Map(ex);
            var safeDetails = masker.Mask(raycynixException.SecureDetails);
            var traceId = httpContext.TraceIdentifier;

            logger.LogError(ex, "Error {Code}: {Msg}. TraceId: {TraceId}. Details: {@Details}",
                raycynixException.ErrorCode, raycynixException.Message, traceId, safeDetails);

            var response = new DefaultExceptionResponse(
                raycynixException.ErrorCode,
                raycynixException.Message,
                TraceId: traceId);

            httpContext.Response.StatusCode = raycynixException.StatusCode;
            httpContext.Response.ContentType = "application/json";

            var json = JsonSerializer.Serialize(response);
            await httpContext.Response.WriteAsync(json);
        }
    }
}
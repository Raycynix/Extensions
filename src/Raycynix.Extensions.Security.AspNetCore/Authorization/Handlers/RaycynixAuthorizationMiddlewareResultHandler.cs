using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Models;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization.Handlers;

/// <summary>
/// Provides a consistent HTTP response for authorization failures without exposing internal policy details.
/// </summary>
public sealed class RaycynixAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private static readonly AuthorizationMiddlewareResultHandler DefaultHandler = new();
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Forbidden)
        {
            await WriteResponseAsync(
                context,
                StatusCodes.Status403Forbidden,
                "forbidden",
                "You do not have permission to access this resource.");

            return;
        }

        if (authorizeResult.Challenged)
        {
            await WriteResponseAsync(
                context,
                StatusCodes.Status401Unauthorized,
                "unauthorized",
                "Authentication is required to access this resource.");

            return;
        }

        await DefaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }

    private static async Task WriteResponseAsync(
        HttpContext context,
        int statusCode,
        string code,
        string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        var response = new AuthorizationErrorResponse(
            Status: statusCode,
            Code: code,
            Message: message,
            TraceId: context.TraceIdentifier);

        var json = JsonSerializer.Serialize(response, SerializerOptions);
        await context.Response.WriteAsync(json);
    }
}

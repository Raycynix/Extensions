using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Handlers;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Models;

namespace Raycynix.Extensions.Security.AspNetCore.Tests.Authorization;

/// <summary>
/// Covers unified HTTP responses for authorization failures.
/// </summary>
public class RaycynixAuthorizationMiddlewareResultHandlerTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Verifies that forbidden results produce the standard <c>403</c> response payload.
    /// </summary>
    [Fact]
    public async Task HandleAsync_ShouldWriteForbiddenResponse()
    {
        var handler = new RaycynixAuthorizationMiddlewareResultHandler();
        var context = CreateHttpContext();

        await handler.HandleAsync(
            _ => Task.CompletedTask,
            context,
            new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build(),
            PolicyAuthorizationResult.Forbid());

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        var payload = await ReadResponseAsync(context);
        payload.Code.Should().Be("forbidden");
        payload.Status.Should().Be(StatusCodes.Status403Forbidden);
    }

    /// <summary>
    /// Verifies that challenged results produce the standard <c>401</c> response payload.
    /// </summary>
    [Fact]
    public async Task HandleAsync_ShouldWriteUnauthorizedResponse()
    {
        var handler = new RaycynixAuthorizationMiddlewareResultHandler();
        var context = CreateHttpContext();

        await handler.HandleAsync(
            _ => Task.CompletedTask,
            context,
            new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build(),
            PolicyAuthorizationResult.Challenge());

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        var payload = await ReadResponseAsync(context);
        payload.Code.Should().Be("unauthorized");
        payload.Status.Should().Be(StatusCodes.Status401Unauthorized);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        return new DefaultHttpContext
        {
            Response =
            {
                Body = new MemoryStream()
            }
        };
    }

    private static async Task<AuthorizationErrorResponse> ReadResponseAsync(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();

        return JsonSerializer.Deserialize<AuthorizationErrorResponse>(json, SerializerOptions)!;
    }
}

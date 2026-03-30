using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Common.Context;

namespace Raycynix.Extensions.Exceptions.AspNetCore.Tests.Middleware;

/// <summary>
/// Covers the ASP.NET Core exception middleware behavior.
/// </summary>
public sealed class RaycynixExceptionMiddlewareTests
{
    /// <summary>
    /// Verifies that mapped exceptions are converted into structured JSON responses.
    /// </summary>
    [Fact]
    public async Task Middleware_ShouldReturnStructuredJson_ForMappedExceptions()
    {
        await using var app = await CreateApp(app =>
        {
            app.UseRaycynixExceptions();
            app.Run(_ => throw new NotFoundException("Missing entity"));
        });

        var response = await app.GetTestClient().GetAsync("/entities/1", TestContext.Current.CancellationToken);
        var payload = await ReadPayloadAsync(response);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        payload.GetProperty("message").GetString().Should().Be("Missing entity");
        payload.GetProperty("errorCode").GetString().Should().Be("RESOURCE_NOT_FOUND");
        payload.GetProperty("category").GetString().Should().Be("notfound");
        payload.GetProperty("path").GetString().Should().Be("/entities/1");
        payload.GetProperty("method").GetString().Should().Be("GET");
    }

    /// <summary>
    /// Verifies that transient failures set the Retry-After header and response metadata.
    /// </summary>
    [Fact]
    public async Task Middleware_ShouldSetRetryAfter_ForTransientFailures()
    {
        await using var app = await CreateApp(app =>
        {
            app.UseRaycynixExceptions();
            app.Run(_ => throw new TransientFailureException("Temporary", retryAfterSeconds: 5));
        });

        var response = await app.GetTestClient().GetAsync("/retry", TestContext.Current.CancellationToken);
        var payload = await ReadPayloadAsync(response);

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        response.Headers.RetryAfter.Should().NotBeNull();
        response.Headers.RetryAfter!.Delta.Should().Be(TimeSpan.FromSeconds(5));
        payload.GetProperty("isTransient").GetBoolean().Should().BeTrue();
        payload.GetProperty("retryAfterSeconds").GetInt32().Should().Be(5);
    }

    /// <summary>
    /// Verifies that validation exceptions include validation errors in the payload.
    /// </summary>
    [Fact]
    public async Task Middleware_ShouldIncludeValidationErrors()
    {
        await using var app = await CreateApp(app =>
        {
            app.UseRaycynixExceptions();
            app.Run(_ => throw new ValidationException("Invalid request", new Dictionary<string, string[]>
            {
                ["Name"] = ["Required"]
            }));
        });

        var response = await app.GetTestClient().PostAsync("/validation", null, TestContext.Current.CancellationToken);
        var payload = await ReadPayloadAsync(response);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var validationErrors = payload.GetProperty("validationErrors");
        validationErrors.TryGetProperty("Name", out var nameErrors).Should().BeTrue();
        nameErrors.EnumerateArray().Select(x => x.GetString()).Should().ContainSingle().Which.Should().Be("Required");
    }

    /// <summary>
    /// Verifies that client-aborted requests do not produce an error response.
    /// </summary>
    [Fact]
    public async Task Middleware_ShouldNotWriteResponse_ForAbortedRequests()
    {
        var httpContext = new DefaultHttpContext();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        httpContext.RequestAborted = cts.Token;

        var middleware = new Raycynix.Extensions.Exceptions.AspNetCore.Middleware.RaycynixExceptionMiddleware(
            _ => throw new OperationCanceledException(cts.Token),
            new Defaults.ExceptionMapper(new Dictionary<Type, Func<Exception, Abstractions.RaycynixException>>()),
            new Defaults.ExceptionDataMasker(),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<Raycynix.Extensions.Exceptions.AspNetCore.Middleware.RaycynixExceptionMiddleware>.Instance);

        var operationContext = new OperationContext { CorrelationId = "corr-1" };

        var act = () => middleware.InvokeAsync(httpContext, operationContext);

        await act.Should().NotThrowAsync();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    private static async Task<WebApplication> CreateApp(Action<IApplicationBuilder> configureApp)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddLogging();
        builder.Services.AddSingleton<IOperationContext>(_ => new OperationContext
        {
            CorrelationId = "corr-1",
            UserId = "user-1"
        });
        builder.Services.AddRaycynixExceptions();

        var app = builder.Build();
        configureApp(app);
        await app.StartAsync(TestContext.Current.CancellationToken);

        return app;
    }

    private static async Task<JsonElement> ReadPayloadAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: TestContext.Current.CancellationToken);
        return document.RootElement.Clone();
    }
}

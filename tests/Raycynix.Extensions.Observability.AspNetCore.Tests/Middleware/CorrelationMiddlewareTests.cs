using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Common.Context;
using Raycynix.Extensions.Observability.AspNetCore.Middleware;
using Raycynix.Extensions.Observability.AspNetCore.Tests.Http;
using Raycynix.Extensions.Security.Abstractions.Enums;
using Raycynix.Extensions.Security.Abstractions.Interfaces;

namespace Raycynix.Extensions.Observability.AspNetCore.Tests.Middleware;

/// <summary>
/// Covers the ASP.NET Core observability middleware pipeline and endpoint mapping.
/// </summary>
public class CorrelationMiddlewareTests
{
    /// <summary>
    /// Verifies that the full observability pipeline writes the correlation response header
    /// and exposes the current operation context during request handling.
    /// </summary>
    [Fact]
    public async Task UseRaycynixObservability_ShouldPopulateResponseHeaderAndOperationContext()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddRaycynixAspNetCoreObservability();

        var app = builder.Build();
        app.UseRaycynixObservability();
        app.MapGet("/context", (HttpContext _, IOperationContext operationContext) =>
        {
            var activity = Activity.Current;

            return Results.Json(new
            {
                operationContext.CorrelationId,
                operationContext.UserId,
                operationContext.SubjectId,
                operationContext.SubjectType,
                TraceId = activity?.GetTagItem("trace.id")?.ToString(),
                CorrelationTag = activity?.GetTagItem("correlation.id")?.ToString(),
                UserTag = activity?.GetTagItem("user.id")?.ToString(),
                SubjectTag = activity?.GetTagItem("subject.id")?.ToString(),
                SubjectTypeTag = activity?.GetTagItem("subject.type")?.ToString(),
                AmbientCorrelationId = OperationContext.Current?.CorrelationId
            });
        });

        await app.StartAsync(TestContext.Current.CancellationToken);

        var client = app.GetTestClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/context");
        request.Headers.Add(CorrelationHeaderHandlerTests.CorrelationHeader, "request-correlation");

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        var payload = await response.Content.ReadFromJsonAsync<CorrelationPayload>(cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.GetValues(CorrelationHeaderHandlerTests.CorrelationHeader).Should().ContainSingle("request-correlation");
        payload.Should().NotBeNull();
        payload.CorrelationId.Should().Be("request-correlation");
        payload.TraceId.Should().NotBeNullOrWhiteSpace();
        payload.CorrelationTag.Should().Be("request-correlation");
        payload.AmbientCorrelationId.Should().Be("request-correlation");
    }

    /// <summary>
    /// Verifies that the correlation middleware copies authenticated user and subject data
    /// into both the operation context and the current activity tags.
    /// </summary>
    [Fact]
    public async Task CorrelationMiddleware_ShouldPopulateUserAndSubjectData_WhenSecurityContextExists()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "alice")], "test")),
            TraceIdentifier = "trace-identifier"
        };

        var operationContext = new OperationContext();
        var services = new ServiceCollection();
        services.AddSingleton<ISecurityContext>(new TestSecurityContext(true, "subject-42", SecuritySubjectType.User));

        var provider = services.BuildServiceProvider();
        var middleware = new CorrelationMiddleware(_ => Task.CompletedTask);

        using var activity = new Activity("test-request").Start();

        await middleware.InvokeAsync(httpContext, operationContext, provider);

        operationContext.CorrelationId.Should().Be("trace-identifier");
        operationContext.UserId.Should().Be("alice");
        operationContext.SubjectId.Should().Be("subject-42");
        operationContext.SubjectType.Should().Be(nameof(SecuritySubjectType.User));
        httpContext.Response.Headers[CorrelationHeaderHandlerTests.CorrelationHeader].ToString().Should().Be("trace-identifier");
        Activity.Current?.GetTagItem("correlation.id")?.ToString().Should().Be("trace-identifier");
        Activity.Current?.GetTagItem("user.id")?.ToString().Should().Be("alice");
        Activity.Current?.GetTagItem("subject.id")?.ToString().Should().Be("subject-42");
        Activity.Current?.GetTagItem("subject.type")?.ToString().Should().Be(nameof(SecuritySubjectType.User));
        OperationContext.Current.Should().BeNull();
    }

    /// <summary>
    /// Verifies that the observability endpoint mapping exposes both health and metrics endpoints.
    /// </summary>
    [Fact]
    public async Task MapRaycynixObservabilityEndpoints_ShouldExposeHealthAndMetricsEndpoints()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddRaycynixAspNetCoreObservability();

        var app = builder.Build();
        app.UseRaycynixObservability();
        app.MapRaycynixObservabilityEndpoints("/internal/health", "/internal/metrics");

        await app.StartAsync(TestContext.Current.CancellationToken);

        var client = app.GetTestClient();

        var healthResponse = await client.GetAsync("/internal/health", TestContext.Current.CancellationToken);
        var metricsResponse = await client.GetAsync("/internal/metrics", TestContext.Current.CancellationToken);

        healthResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        metricsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Represents the payload returned by the test endpoint for correlation assertions.
    /// </summary>
    private sealed record CorrelationPayload(
        string CorrelationId,
        string? UserId,
        string? SubjectId,
        string? SubjectType,
        string? TraceId,
        string? CorrelationTag,
        string? UserTag,
        string? SubjectTag,
        string? SubjectTypeTag,
        string? AmbientCorrelationId);

    /// <summary>
    /// Provides a minimal authenticated security context for middleware tests.
    /// </summary>
    private sealed class TestSecurityContext(bool isAuthenticated, string subjectId, SecuritySubjectType subjectType)
        : ISecurityContext
    {
        public bool IsAuthenticated { get; } = isAuthenticated;
        public string SubjectId { get; } = subjectId;
        public SecuritySubjectType SubjectType { get; } = subjectType;
        public IReadOnlyCollection<string> Roles { get; } = [];
        public IReadOnlyCollection<string> Permissions { get; } = [];
    }
}

using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Common.Context;
using Raycynix.Extensions.Observability.AspNetCore.Configurations;
using Raycynix.Extensions.Observability.AspNetCore.Middleware;
using Raycynix.Extensions.Metrics.AspNetCore;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
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

    [Fact]
    public async Task CorrelationMiddleware_ShouldReplaceInvalidIncomingCorrelationId()
    {
        var httpContext = new DefaultHttpContext { TraceIdentifier = "trusted-trace" };
        httpContext.Request.Headers[CorrelationHeaderHandlerTests.CorrelationHeader] = "invalid correlation value";
        var operationContext = new OperationContext();
        using var provider = new ServiceCollection().BuildServiceProvider();
        var middleware = new CorrelationMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(httpContext, operationContext, provider);

        operationContext.CorrelationId.Should().Be("trusted-trace");
        httpContext.Response.Headers[CorrelationHeaderHandlerTests.CorrelationHeader]
            .ToString().Should().Be("trusted-trace");
    }

    [Fact]
    public async Task CorrelationMiddleware_ShouldRejectMultipleIncomingCorrelationIds()
    {
        var httpContext = new DefaultHttpContext { TraceIdentifier = "trusted-trace" };
        httpContext.Request.Headers.Append(CorrelationHeaderHandlerTests.CorrelationHeader, "first");
        httpContext.Request.Headers.Append(CorrelationHeaderHandlerTests.CorrelationHeader, "second");
        var operationContext = new OperationContext();
        using var provider = new ServiceCollection().BuildServiceProvider();
        var middleware = new CorrelationMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(httpContext, operationContext, provider);

        operationContext.CorrelationId.Should().Be("trusted-trace");
    }

    [Fact]
    public async Task CorrelationMiddleware_ShouldReplaceInvalidPreexistingOperationCorrelationId()
    {
        var httpContext = new DefaultHttpContext { TraceIdentifier = "trusted-trace" };
        var operationContext = new OperationContext { CorrelationId = "invalid correlation value" };
        using var provider = new ServiceCollection().BuildServiceProvider();
        var middleware = new CorrelationMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(httpContext, operationContext, provider);

        operationContext.CorrelationId.Should().HaveLength(32);
        operationContext.CorrelationId.Should().NotBe("invalid correlation value");
    }

    /// <summary>
    /// Verifies that downstream log events keep the identity values resolved by the correlation middleware.
    /// </summary>
    [Fact]
    public async Task CorrelationMiddleware_ShouldPreserveUserAndSubjectData_InLoggingScope()
    {
        var loggerProvider = new CollectingLoggerProvider();
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "alice")], "test")),
            TraceIdentifier = "trace-identifier"
        };

        var operationContext = new OperationContext();
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddProvider(loggerProvider));
        services.AddSingleton<ISecurityContext>(new TestSecurityContext(true, "subject-42", SecuritySubjectType.User));
        httpContext.RequestServices = services.BuildServiceProvider();

        var middleware = new CorrelationMiddleware(context =>
        {
            context.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("downstream")
                .LogInformation("handled");

            return Task.CompletedTask;
        });

        using var activity = new Activity("test-request").Start();
        var expectedTraceId = Activity.Current?.TraceId.ToString();

        await middleware.InvokeAsync(httpContext, operationContext, httpContext.RequestServices);

        loggerProvider.Entries.Should().Contain(entry => ScopeExists(
            entry,
            "trace-identifier",
            expectedTraceId,
            "alice",
            "subject-42",
            nameof(SecuritySubjectType.User)));
    }

    /// <summary>
    /// Verifies that identity values can be omitted from logging scopes without changing operation context enrichment.
    /// </summary>
    [Fact]
    public async Task CorrelationMiddleware_ShouldOmitUserAndSubjectData_FromLoggingScope_WhenDisabled()
    {
        var loggerProvider = new CollectingLoggerProvider();
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "alice")], "test")),
            TraceIdentifier = "trace-identifier"
        };

        var operationContext = new OperationContext();
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddProvider(loggerProvider));
        services.AddSingleton<ISecurityContext>(new TestSecurityContext(true, "subject-42", SecuritySubjectType.User));
        httpContext.RequestServices = services.BuildServiceProvider();

        var middleware = new CorrelationMiddleware(
            context =>
            {
                context.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("downstream")
                    .LogInformation("handled");

                return Task.CompletedTask;
            },
            Options.Create(new ObservabilityAspNetCoreConfiguration
            {
                IncludeIdentityInLoggingScope = false
            }));

        using var activity = new Activity("test-request").Start();
        var expectedTraceId = Activity.Current?.TraceId.ToString();

        await middleware.InvokeAsync(httpContext, operationContext, httpContext.RequestServices);

        operationContext.UserId.Should().Be("alice");
        operationContext.SubjectId.Should().Be("subject-42");
        loggerProvider.Entries.Should().Contain(entry => ScopeExistsWithoutIdentity(
            entry,
            "trace-identifier",
            expectedTraceId));
    }

    /// <summary>
    /// Verifies that health mapping and an explicitly selected metrics exporter compose correctly.
    /// </summary>
    [Fact]
    public async Task ObservabilityEndpoints_ShouldComposeWithExplicitPrometheusExporter()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddRaycynixAspNetCoreObservability();
        builder.Services.AddOpenTelemetry().WithMetrics(metrics => metrics.AddPrometheusExporter());

        var app = builder.Build();
        app.UseRaycynixObservability();
        app.UseOpenTelemetryPrometheusScrapingEndpoint(
            context => context.Request.Path == "/internal/metrics");
        app.MapRaycynixObservabilityEndpoints("/internal/health");

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

    private sealed class CollectingLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private IExternalScopeProvider _scopeProvider = new LoggerExternalScopeProvider();

        public List<LogEntry> Entries { get; } = [];

        public ILogger CreateLogger(string categoryName) => new CollectingLogger(this);

        public void Dispose()
        {
        }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        private sealed class CollectingLogger(CollectingLoggerProvider provider) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>
                provider._scopeProvider.Push(state);

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                var scopes = new List<IReadOnlyDictionary<string, object?>>();

                provider._scopeProvider.ForEachScope(
                    (scope, state) =>
                    {
                        if (scope is IEnumerable<KeyValuePair<string, object?>> values)
                            state.Add(values.ToDictionary(pair => pair.Key, pair => pair.Value));
                    },
                    scopes);

                provider.Entries.Add(new LogEntry(scopes));
            }
        }
    }

    private sealed record LogEntry(IReadOnlyCollection<IReadOnlyDictionary<string, object?>> Scopes);

    private static bool ScopeExists(
        LogEntry entry,
        string expectedCorrelationId,
        string? expectedTraceId,
        string expectedUserId,
        string expectedSubjectId,
        string expectedSubjectType)
    {
        return entry.Scopes.Any(scope =>
        {
            scope.TryGetValue("CorrelationId", out var correlationId);
            scope.TryGetValue("TraceId", out var traceId);
            scope.TryGetValue("UserId", out var userId);
            scope.TryGetValue("SubjectId", out var subjectId);
            scope.TryGetValue("SubjectType", out var subjectType);

            return correlationId?.ToString() == expectedCorrelationId &&
                   traceId?.ToString() == expectedTraceId &&
                   userId?.ToString() == expectedUserId &&
                   subjectId?.ToString() == expectedSubjectId &&
                   subjectType?.ToString() == expectedSubjectType;
        });
    }

    private static bool ScopeExistsWithoutIdentity(
        LogEntry entry,
        string expectedCorrelationId,
        string? expectedTraceId)
    {
        return entry.Scopes.Any(scope =>
        {
            scope.TryGetValue("CorrelationId", out var correlationId);
            scope.TryGetValue("TraceId", out var traceId);

            return correlationId?.ToString() == expectedCorrelationId &&
                   traceId?.ToString() == expectedTraceId &&
                   !scope.ContainsKey("UserId") &&
                   !scope.ContainsKey("SubjectId") &&
                   !scope.ContainsKey("SubjectType");
        });
    }

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

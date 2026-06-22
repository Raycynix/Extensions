using System.Diagnostics;
using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Raycynix.Extensions.Tracing.AspNetCore.Tests.Middleware;

/// <summary>
/// Covers ASP.NET Core tracing middleware behavior.
/// </summary>
public sealed class TracingMiddlewareTests
{
    /// <summary>
    /// Verifies that active activity identifiers are pushed into a standard logging scope.
    /// </summary>
    [Fact]
    public async Task Middleware_ShouldPushTraceAndSpanIds_FromCurrentActivity()
    {
        var provider = new CollectingLoggerProvider();

        await using var app = await CreateApp(
            builder => builder.Services.AddLogging(logging => logging.AddProvider(provider)),
            app =>
            {
                app.UseRaycynixTracing();
                app.Run(context =>
                {
                    context.Response.Headers["X-Current-TraceId"] = Activity.Current?.TraceId.ToString();
                    context.Response.Headers["X-Current-SpanId"] = Activity.Current?.SpanId.ToString();
                    var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
                    loggerFactory.CreateLogger("test").LogInformation("request handled");
                    return Task.CompletedTask;
                });
            });

        var response = await app.GetTestClient().GetAsync("/trace", TestContext.Current.CancellationToken);
        var currentTraceId = response.Headers.GetValues("X-Current-TraceId").Single();
        var currentSpanId = response.Headers.GetValues("X-Current-SpanId").Single();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        provider.Entries.Should().Contain(entry => ScopeExists(entry, currentTraceId, currentSpanId));
    }

    /// <summary>
    /// Verifies that middleware falls back to HttpContext.TraceIdentifier when no activity exists.
    /// </summary>
    [Fact]
    public async Task Middleware_ShouldFallbackToHttpContextTraceIdentifier_WhenNoActivityExists()
    {
        var provider = new CollectingLoggerProvider();
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(provider));
        var middleware = new Raycynix.Extensions.Tracing.AspNetCore.Middleware.TracingMiddleware(_ =>
        {
            loggerFactory.CreateLogger("test").LogInformation("request handled");
            return Task.CompletedTask;
        }, loggerFactory);
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "trace-id-from-context"
        };

        await middleware.InvokeAsync(context);

        provider.Entries.Should().Contain(entry => ScopeExists(entry, "trace-id-from-context", null));
    }

    private static async Task<WebApplication> CreateApp(
        Action<WebApplicationBuilder> configureBuilder,
        Action<WebApplication> configure)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        configureBuilder(builder);

        var app = builder.Build();
        configure(app);
        await app.StartAsync(TestContext.Current.CancellationToken);

        return app;
    }

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

    private static bool ScopeExists(LogEntry entry, string expectedTraceId, string? expectedSpanId)
    {
        return entry.Scopes.Any(scope =>
        {
            scope.TryGetValue("TraceId", out var traceId);
            scope.TryGetValue("SpanId", out var spanId);

            return traceId?.ToString() == expectedTraceId &&
                   spanId?.ToString() == expectedSpanId;
        });
    }
}
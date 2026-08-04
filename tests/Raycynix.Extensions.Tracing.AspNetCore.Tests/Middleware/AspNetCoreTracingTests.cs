using System.Diagnostics;
using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Raycynix.Extensions.Tracing.AspNetCore.Tests.Middleware;

/// <summary>
/// Covers standard ASP.NET Core activity and logging correlation behavior.
/// </summary>
public sealed class AspNetCoreTracingTests
{
    [Fact]
    public async Task Request_ShouldExposeActivity_AndStandardTraceLoggingScope()
    {
        var loggingProvider = new CollectingLoggerProvider();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddLogging(logging => logging.AddProvider(loggingProvider));
        builder.Services.AddRaycynixAspNetCoreTracing();

        var app = builder.Build();
        app.Run(context =>
        {
            context.Response.Headers["X-Current-TraceId"] = Activity.Current?.TraceId.ToString();
            context.Response.Headers["X-Current-SpanId"] = Activity.Current?.SpanId.ToString();
            context.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("test")
                .LogInformation("request handled");
            return Task.CompletedTask;
        });
        await app.StartAsync(TestContext.Current.CancellationToken);

        var response = await app.GetTestClient().GetAsync("/trace", TestContext.Current.CancellationToken);
        var traceId = response.Headers.GetValues("X-Current-TraceId").Single();
        var spanId = response.Headers.GetValues("X-Current-SpanId").Single();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        traceId.Should().NotBeNullOrWhiteSpace();
        spanId.Should().NotBeNullOrWhiteSpace();
        loggingProvider.Entries.Should().Contain(entry => ScopeExists(entry, traceId, spanId));
    }

    private sealed class CollectingLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private IExternalScopeProvider _scopeProvider = new LoggerExternalScopeProvider();

        public List<LogEntry> Entries { get; } = [];

        public ILogger CreateLogger(string categoryName) => new CollectingLogger(this);

        public void Dispose()
        {
        }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider) => _scopeProvider = scopeProvider;

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
                    (scope, target) =>
                    {
                        if (scope is IEnumerable<KeyValuePair<string, object?>> values)
                        {
                            target.Add(values.ToDictionary(pair => pair.Key, pair => pair.Value));
                        }
                    },
                    scopes);
                provider.Entries.Add(new(scopes));
            }
        }
    }

    private sealed record LogEntry(IReadOnlyCollection<IReadOnlyDictionary<string, object?>> Scopes);

    private static bool ScopeExists(LogEntry entry, string traceId, string spanId)
    {
        return entry.Scopes.Any(scope =>
            scope.TryGetValue("TraceId", out var actualTraceId) &&
            scope.TryGetValue("SpanId", out var actualSpanId) &&
            actualTraceId?.ToString() == traceId &&
            actualSpanId?.ToString() == spanId);
    }
}

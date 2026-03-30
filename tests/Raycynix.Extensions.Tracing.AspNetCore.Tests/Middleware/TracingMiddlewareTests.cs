using System.Diagnostics;
using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Raycynix.Extensions.Tracing.AspNetCore.Tests.Middleware;

/// <summary>
/// Covers ASP.NET Core tracing middleware behavior.
/// </summary>
public sealed class TracingMiddlewareTests
{
    /// <summary>
    /// Verifies that active activity identifiers are pushed into Serilog log context.
    /// </summary>
    [Fact]
    public async Task Middleware_ShouldPushTraceAndSpanIds_FromCurrentActivity()
    {
        var sink = new CollectingSink();
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Sink(sink)
            .CreateLogger();

        await using var app = await CreateApp(app =>
        {
            app.UseRaycynixTracing();
            app.Run(context =>
            {
                context.Response.Headers["X-Current-TraceId"] = Activity.Current?.TraceId.ToString();
                context.Response.Headers["X-Current-SpanId"] = Activity.Current?.SpanId.ToString();
                Log.Logger.Information("request handled");
                return Task.CompletedTask;
            });
        });

        var response = await app.GetTestClient().GetAsync("/trace", TestContext.Current.CancellationToken);
        var currentTraceId = response.Headers.GetValues("X-Current-TraceId").Single();
        var currentSpanId = response.Headers.GetValues("X-Current-SpanId").Single();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        sink.Events.Should().ContainSingle();
        sink.Events.Single().Properties["TraceId"].ToString().Trim('"').Should().Be(currentTraceId);
        sink.Events.Single().Properties["SpanId"].ToString().Trim('"').Should().Be(currentSpanId);
    }

    /// <summary>
    /// Verifies that middleware falls back to HttpContext.TraceIdentifier when no activity exists.
    /// </summary>
    [Fact]
    public async Task Middleware_ShouldFallbackToHttpContextTraceIdentifier_WhenNoActivityExists()
    {
        var sink = new CollectingSink();
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Sink(sink)
            .CreateLogger();
        var middleware = new Raycynix.Extensions.Tracing.AspNetCore.Middleware.TracingMiddleware(_ =>
        {
            Log.Logger.Information("request handled");
            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext();
        context.TraceIdentifier = "trace-id-from-context";

        await middleware.InvokeAsync(context);

        sink.Events.Should().ContainSingle();
        sink.Events.Single().Properties["TraceId"].ToString().Trim('"').Should().Be("trace-id-from-context");
        sink.Events.Single().Properties["SpanId"].ToString().Should().Be("null");
    }

    private static async Task<WebApplication> CreateApp(Action<WebApplication> configure)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        var app = builder.Build();
        configure(app);
        await app.StartAsync(TestContext.Current.CancellationToken);

        return app;
    }

    private sealed class CollectingSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = [];

        public void Emit(LogEvent logEvent)
        {
            Events.Add(logEvent);
        }
    }
}

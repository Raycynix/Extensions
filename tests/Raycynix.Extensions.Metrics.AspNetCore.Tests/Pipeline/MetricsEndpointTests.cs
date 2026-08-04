using System.Diagnostics.Metrics;
using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using Raycynix.Extensions.Metrics.Abstractions;

namespace Raycynix.Extensions.Metrics.AspNetCore.Tests.Pipeline;

/// <summary>
/// Covers OpenTelemetry ASP.NET Core and Prometheus integration.
/// </summary>
public sealed class MetricsEndpointTests
{
    [Fact]
    public async Task PrometheusEndpoint_ShouldExposeRaycynixInstruments()
    {
        await using var app = await CreateApp(app =>
            app.UseOpenTelemetryPrometheusScrapingEndpoint());

        var response = await app.GetTestClient().GetAsync("/metrics", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("raycynix_test_requests_total");
    }

    [Fact]
    public async Task PrometheusEndpoint_ShouldUseCustomExactPath()
    {
        await using var app = await CreateApp(app =>
            app.UseOpenTelemetryPrometheusScrapingEndpoint(
                context => context.Request.Path == "/internal/metrics"));

        var customResponse = await app.GetTestClient()
            .GetAsync("/internal/metrics", TestContext.Current.CancellationToken);
        var defaultResponse = await app.GetTestClient()
            .GetAsync("/metrics", TestContext.Current.CancellationToken);

        customResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        defaultResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PrometheusMiddleware_ShouldPreserveRegularEndpoints()
    {
        await using var app = await CreateApp(app =>
        {
            app.UseOpenTelemetryPrometheusScrapingEndpoint();
            app.MapGet("/health", () => TypedResults.Ok("healthy"));
        });

        var response = await app.GetTestClient().GetAsync("/health", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Be("\"healthy\"");
    }

    [Fact]
    public void AddRaycynixAspNetCoreMetrics_ShouldInvokeExporterConfiguration()
    {
        var services = new ServiceCollection();
        var invoked = false;

        services.AddRaycynixAspNetCoreMetrics(_ => invoked = true);

        invoked.Should().BeTrue();
    }

    private static async Task<WebApplication> CreateApp(Action<WebApplication> configure)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddRaycynixAspNetCoreMetrics(metrics => metrics.AddPrometheusExporter());

        var app = builder.Build();
        var meter = RaycynixMetrics.CreateMeter(app.Services.GetRequiredService<IMeterFactory>());
        configure(app);
        await app.StartAsync(TestContext.Current.CancellationToken);

        var counter = meter.CreateCounter<long>("raycynix.test.requests", "{request}");
        counter.Add(1);

        return app;
    }
}

using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;

namespace Raycynix.Extensions.Metrics.AspNetCore.Tests.Pipeline;

/// <summary>
/// Covers ASP.NET Core middleware and endpoint integration for metrics.
/// </summary>
public sealed class MetricsEndpointTests
{
    /// <summary>
    /// Verifies that the default metrics endpoint is exposed when mapped.
    /// </summary>
    [Fact]
    public async Task MapRaycynixMetrics_ShouldExposeDefaultMetricsEndpoint()
    {
        await using var app = await CreateApp(app =>
        {
            app.UseRaycynixMetrics();
            app.MapRaycynixMetrics();
        });

        var response = await app.GetTestClient().GetAsync("/metrics", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("# HELP");
        content.Should().Contain("# TYPE");
    }

    /// <summary>
    /// Verifies that a custom metrics path can be mapped.
    /// </summary>
    [Fact]
    public async Task MapRaycynixMetrics_ShouldUseCustomPath_WhenProvided()
    {
        await using var app = await CreateApp(app =>
        {
            app.UseRaycynixMetrics();
            app.MapRaycynixMetrics("/internal/metrics");
        });

        var response = await app.GetTestClient().GetAsync("/internal/metrics", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that the metrics middleware does not interfere with normal application routes.
    /// </summary>
    [Fact]
    public async Task UseRaycynixMetrics_ShouldPreserveRegularEndpoints()
    {
        await using var app = await CreateApp(app =>
        {
            app.UseRaycynixMetrics();
            app.MapGet("/health", () => TypedResults.Ok("healthy"));
        });

        var response = await app.GetTestClient().GetAsync("/health", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Be("\"healthy\"");
    }

    private static async Task<WebApplication> CreateApp(Action<WebApplication> configure)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddRaycynixMetrics();

        var app = builder.Build();
        configure(app);
        await app.StartAsync(TestContext.Current.CancellationToken);

        return app;
    }
}

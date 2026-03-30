using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;

namespace Raycynix.Extensions.Configuration.AspNetCore.Tests.FeatureGate;

/// <summary>
/// Covers feature gate behavior for endpoint metadata and middleware.
/// </summary>
public class FeatureGateEndpointTests
{
    /// <summary>
    /// Verifies that endpoints guarded by <c>RequireFeature</c> return <c>404</c> when the feature is disabled.
    /// </summary>
    [Fact]
    public async Task RequireFeature_ShouldReturnNotFoundWhenFeatureIsDisabled()
    {
        await using var app = await BuildFeatureGateAppAsync(new Dictionary<string, string?>
        {
            ["FeatureFlags:Flags:NewDashboard"] = "false"
        }, endpoint =>
        {
            endpoint.RequireFeature("NewDashboard");
        });

        var response = await app.GetTestClient().GetAsync("/feature", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that endpoints guarded by <c>RequireFeature</c> return <c>200</c> when the feature is enabled.
    /// </summary>
    [Fact]
    public async Task RequireFeature_ShouldAllowRequestWhenFeatureIsEnabled()
    {
        await using var app = await BuildFeatureGateAppAsync(new Dictionary<string, string?>
        {
            ["FeatureFlags:Flags:NewDashboard"] = "true"
        }, endpoint =>
        {
            endpoint.RequireFeature("NewDashboard");
        });

        var response = await app.GetTestClient().GetAsync("/feature", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that endpoints guarded by <c>RequireAnyFeature</c> allow the request when at least one flag is enabled.
    /// </summary>
    [Fact]
    public async Task RequireAnyFeature_ShouldAllowRequestWhenAtLeastOneFeatureIsEnabled()
    {
        await using var app = await BuildFeatureGateAppAsync(new Dictionary<string, string?>
        {
            ["FeatureFlags:Flags:Alpha"] = "false",
            ["FeatureFlags:Flags:Beta"] = "true"
        }, endpoint =>
        {
            endpoint.RequireAnyFeature("Alpha", "Beta");
        });

        var response = await app.GetTestClient().GetAsync("/feature", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that feature gate attributes are also respected by the middleware.
    /// </summary>
    [Fact]
    public async Task FeatureGateAttribute_ShouldReturnNotFoundWhenRequiredFeatureIsDisabled()
    {
        await using var app = await BuildFeatureGateAppAsync(new Dictionary<string, string?>
        {
            ["FeatureFlags:Flags:FlaggedEndpoint"] = "false"
        }, endpoint =>
        {
            endpoint.WithMetadata(new Raycynix.Extensions.Configuration.AspNetCore.FeatureGate.FeatureGateAttribute("FlaggedEndpoint"));
        });

        var response = await app.GetTestClient().GetAsync("/feature", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that multiple feature gates on the same endpoint are all evaluated by the middleware.
    /// </summary>
    [Fact]
    public async Task MultipleFeatureGates_ShouldReturnNotFoundWhenAnyGateIsNotSatisfied()
    {
        await using var app = await BuildFeatureGateAppAsync(new Dictionary<string, string?>
        {
            ["FeatureFlags:Flags:First"] = "true",
            ["FeatureFlags:Flags:Second"] = "false"
        }, endpoint =>
        {
            endpoint.RequireFeature("First");
            endpoint.RequireFeature("Second");
        });

        var response = await app.GetTestClient().GetAsync("/feature", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    private static async Task<WebApplication> BuildFeatureGateAppAsync(
        IDictionary<string, string?> flags,
        Action<RouteHandlerBuilder> configureEndpoint)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(flags);
        builder.Services.AddRaycynixFeatureFlags(builder.Configuration);

        var app = builder.Build();
        app.UseRaycynixAspNetCoreConfiguration();

        var endpoint = app.MapGet("/feature", () => Results.Ok("enabled"));
        configureEndpoint(endpoint);

        await app.StartAsync(TestContext.Current.CancellationToken);

        return app;
    }
}

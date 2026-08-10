using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Contracts.AspNetCore.Attributes;
using Raycynix.Extensions.Contracts.AspNetCore.Metadata;
using Raycynix.Extensions.Contracts.Models;

namespace Raycynix.Extensions.Contracts.AspNetCore.Tests.Pipeline;

/// <summary>
/// Covers endpoint metadata, middleware, and result helper integration.
/// </summary>
public sealed class ContractEndpointTests
{
    /// <summary>
    /// Verifies that minimal API endpoints expose declared contract metadata.
    /// </summary>
    [Fact]
    public async Task WithContract_ShouldAttachEndpointMetadata()
    {
        await using var app = await CreateApp(app =>
        {
            app.MapGet("/prices", TypedResults.Ok)
                .WithContract("catalog.prices", "1.2.0");
        });

        var endpoint = app.Services.GetRequiredService<EndpointDataSource>().Endpoints.Single();
        var metadata = endpoint.Metadata.GetMetadata<ContractEndpointMetadata>();

        metadata.Should().NotBeNull();
        metadata!.Metadata.Name.Should().Be("catalog.prices");
        metadata.Metadata.Version.ToString().Should().Be("1.2.0");
    }

    /// <summary>
    /// Verifies that contract middleware writes response headers from declared endpoint metadata.
    /// </summary>
    [Fact]
    public async Task Middleware_ShouldWriteHeaders_ForDeclaredEndpointContract()
    {
        await using var app = await CreateApp(app =>
        {
            app.UseRaycynixContracts();
            app.MapGet("/prices", () => TypedResults.Ok(new Money { Amount = 149.99m, Currency = "USD" }))
                .WithContract("catalog.prices", "1.2.0");
        });

        var response = await app.GetTestClient().GetAsync("/prices", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.GetValues("X-Contract-Name").Should().ContainSingle().Which.Should().Be("catalog.prices");
        response.Headers.GetValues("X-Contract-Version").Should().ContainSingle().Which.Should().Be("1.2.0");
    }

    /// <summary>
    /// Verifies that HttpContext result helpers reuse endpoint metadata to build a versioned envelope.
    /// </summary>
    [Fact]
    public async Task HttpContextVersionedContract_ShouldReturnVersionedEnvelope()
    {
        await using var app = await CreateApp(app =>
        {
            app.UseRaycynixContracts();
            app.MapGet("/prices", (HttpContext httpContext) =>
                    httpContext.VersionedContract(new Money { Amount = 149.99m, Currency = "USD" }))
                .WithContract("catalog.prices", "1.2.0");
        });

        var response = await app.GetTestClient().GetAsync("/prices", TestContext.Current.CancellationToken);
        var payload = await ReadPayloadAsync(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        payload.GetProperty("metadata").GetProperty("name").GetString().Should().Be("catalog.prices");
        payload.GetProperty("metadata").GetProperty("version").GetProperty("major").GetInt32().Should().Be(1);
        payload.GetProperty("payload").GetProperty("currency").GetString().Should().Be("USD");
    }

    /// <summary>
    /// Verifies that HttpContext contract helpers preserve the plain payload while still using endpoint metadata.
    /// </summary>
    [Fact]
    public async Task HttpContextContract_ShouldReturnPlainPayload()
    {
        await using var app = await CreateApp(app =>
        {
            app.UseRaycynixContracts();
            app.MapGet("/prices/plain", (HttpContext httpContext) =>
                    httpContext.Contract(new Money { Amount = 149.99m, Currency = "USD" }))
                .WithContract("catalog.prices", "1.2.0");
        });

        var response = await app.GetTestClient().GetAsync("/prices/plain", TestContext.Current.CancellationToken);
        var payload = await ReadPayloadAsync(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.GetValues("X-Contract-Name").Should().ContainSingle().Which.Should().Be("catalog.prices");
        payload.TryGetProperty("metadata", out _).Should().BeFalse();
        payload.GetProperty("currency").GetString().Should().Be("USD");
    }

    /// <summary>
    /// Verifies that controller helpers can resolve metadata from the current endpoint.
    /// </summary>
    [Fact]
    public async Task ControllerVersionedContract_ShouldUseEndpointMetadata()
    {
        var context = new DefaultHttpContext();
        context.SetEndpoint(new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(
                new ContractAttribute("catalog.prices", "1.2.0")),
            "catalog.prices"));

        var controller = new TestController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = context
            }
        };

        var result = controller.CreateVersionedContractResult(new Money { Amount = 149.99m, Currency = "USD" });
        await result.ExecuteAsync(context);

        context.Response.Headers["X-Contract-Name"].ToString().Should().Be("catalog.prices");
        context.Response.Headers["X-Contract-Version"].ToString().Should().Be("1.2.0");
    }

    /// <summary>
    /// Verifies that invalid contract declarations fail while endpoints are configured.
    /// </summary>
    [Fact]
    public void WithContract_ShouldRejectMissingContractName()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        var action = () => app.MapGet("/invalid", TypedResults.Ok)
            .WithContract(string.Empty, "1.0.0");

        action.Should().Throw<ArgumentException>();
    }

    private static async Task<WebApplication> CreateApp(Action<WebApplication> configure)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddRaycynixContractsAspNetCore();

        var app = builder.Build();
        configure(app);
        await app.StartAsync(TestContext.Current.CancellationToken);

        return app;
    }

    private static async Task<JsonElement> ReadPayloadAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: TestContext.Current.CancellationToken);
        return document.RootElement.Clone();
    }

    private sealed class TestController : ControllerBase
    {
        public IResult CreateVersionedContractResult(Money payload)
        {
            return this.VersionedContract(payload);
        }
    }
}

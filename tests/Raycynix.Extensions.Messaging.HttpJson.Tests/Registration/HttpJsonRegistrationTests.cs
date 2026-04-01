using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.HttpJson.Configurations;
using Raycynix.Extensions.Messaging.HttpJson.Interfaces;
using Raycynix.Extensions.Messaging.HttpJson.Internal;

namespace Raycynix.Extensions.Messaging.HttpJson.Tests.Registration;

/// <summary>
/// Covers service registration and direct request behavior for the Raycynix HTTP JSON messaging package.
/// </summary>
public sealed class HttpJsonRegistrationTests
{
    /// <summary>
    /// Verifies that HTTP JSON transport registration replaces the default direct client and registers transport options.
    /// </summary>
    [Fact]
    public void AddHttpJson_ShouldRegisterHttpJsonConfigurationAndDirectClient()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddRaycynixMessaging(configuration)
            .AddHttpJson(options =>
            {
                options.BaseAddress = "https://orders.service.local";
                options.TimeoutSeconds = 15;
            });

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<HttpJsonMessagingConfiguration>().BaseAddress.Should().Be("https://orders.service.local");
        provider.GetRequiredService<IDirectRequestClient>().Should().BeOfType<HttpJsonRequestClient>();
    }

    /// <summary>
    /// Verifies that HTTP JSON direct requests serialize the payload, send headers, and deserialize the response.
    /// </summary>
    [Fact]
    public async Task SendAsync_ShouldSerializeRequestAndDeserializeResponse()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var fakeTransport = new FakeHttpJsonTransport();

        services.AddRaycynixMessaging(configuration)
            .AddHttpJson(options =>
            {
                options.BaseAddress = "https://orders.service.local";
                options.TimeoutSeconds = 15;
            });

        services.Replace(ServiceDescriptor.Singleton<IHttpJsonTransport>(fakeTransport));

        await using var provider = services.BuildServiceProvider();
        var envelopeFactory = provider.GetRequiredService<IRequestEnvelopeFactory>();
        var client = provider.GetRequiredService<IDirectRequestClient>();
        var request = envelopeFactory.Create(
            new CreateOrderRequest("order-1"),
            "/api/orders",
            MessageFormat.Json,
            headers: new Dictionary<string, string> { ["X-Tenant-Id"] = "tenant-1" },
            correlationId: "corr-1");

        var response = await client.SendAsync<CreateOrderRequest, CreateOrderResponse>(
            request,
            TestContext.Current.CancellationToken);

        fakeTransport.Request.Should().NotBeNull();
        fakeTransport.Request!.RequestUri.Should().Be(new Uri("/api/orders", UriKind.Relative));
        fakeTransport.Request.Headers.GetValues("X-Request-Id").Should().ContainSingle();
        fakeTransport.Request.Headers.GetValues("X-Correlation-Id").Should().ContainSingle("corr-1");
        fakeTransport.Request.Headers.GetValues("X-Tenant-Id").Should().ContainSingle("tenant-1");
        response.Response.OrderId.Should().Be("order-1");
        response.CorrelationId.Should().Be("corr-1");
    }

    private sealed record CreateOrderRequest(string OrderId);

    private sealed record CreateOrderResponse(string OrderId);

    private sealed class FakeHttpJsonTransport : IHttpJsonTransport
    {
        public HttpRequestMessage? Request { get; private set; }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;

            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{\"orderId\":\"order-1\"}")
            };

            response.Headers.Add("X-Correlation-Id", "corr-1");
            return Task.FromResult(response);
        }
    }
}

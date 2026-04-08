using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Messaging.Abstractions.Attributes;
using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.HttpJson.Configurations;
using Raycynix.Extensions.Messaging.HttpJson.Interfaces;
using Raycynix.Extensions.Messaging.HttpJson.Internal;
using Raycynix.Extensions.Security.Abstractions.Attributes;
using Raycynix.Extensions.Security.Abstractions.Enums;

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
    /// Verifies that HTTP JSON transport options can be bound from configuration using the default section name.
    /// </summary>
    [Fact]
    public void AddHttpJson_WithConfiguration_ShouldBindConfigurationAndRegisterDirectClient()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["HttpJsonMessagingConfiguration:BaseAddress"] = "https://catalog.service.local",
                ["HttpJsonMessagingConfiguration:TimeoutSeconds"] = "45"
            })
            .Build();

        services.AddRaycynixMessaging(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build())
            .AddHttpJson(configuration);

        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<HttpJsonMessagingConfiguration>();
        options.BaseAddress.Should().Be("https://catalog.service.local");
        options.TimeoutSeconds.Should().Be(45);
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

    /// <summary>
    /// Verifies that inbound HTTP JSON requests are dispatched to the registered request handler.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_ShouldDispatchInboundHttpJsonRequest()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddRaycynixMessaging(configuration)
            .AddRequestHandler<CreateOrderRequest, CreateOrderResponse, CreateOrderRequestHandler>("/api/orders")
            .AddHttpJson(options =>
            {
                options.BaseAddress = "https://orders.service.local";
                options.TimeoutSeconds = 15;
            });

        await using var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<IHttpJsonRequestProcessor>();

        var response = await processor.ProcessAsync<CreateOrderRequest, CreateOrderResponse>(
            "/api/orders",
            System.Text.Encoding.UTF8.GetBytes("{\"orderId\":\"order-5\"}"),
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["X-Correlation-Id"] = "corr-5"
            },
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        response.Headers.Should().ContainKey("X-Correlation-Id");
        System.Text.Encoding.UTF8.GetString(response.Payload).Should().Contain("order-5");
    }

    /// <summary>
    /// Verifies that inbound HTTP JSON requests return a not found response when no request handler is registered.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WithoutRegisteredRequestHandler_ShouldReturnNotFound()
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

        await using var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<IHttpJsonRequestProcessor>();

        var response = await processor.ProcessAsync<CreateOrderRequest, CreateOrderResponse>(
            "/api/orders",
            System.Text.Encoding.UTF8.GetBytes("{\"orderId\":\"order-6\"}"),
            cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that malformed inbound security headers are rejected before request dispatch.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WithInvalidSecurityHeaders_ShouldReturnUnauthorized()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddRaycynixMessaging(configuration)
            .AddRequestHandler<CreateOrderRequest, CreateOrderResponse, CreateOrderRequestHandler>("/api/orders")
            .AddHttpJson(options =>
            {
                options.BaseAddress = "https://orders.service.local";
                options.TimeoutSeconds = 15;
            });

        await using var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<IHttpJsonRequestProcessor>();

        var response = await processor.ProcessAsync<CreateOrderRequest, CreateOrderResponse>(
            "/api/orders",
            System.Text.Encoding.UTF8.GetBytes("{\"orderId\":\"order-9\"}"),
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["X-Subject-Authenticated"] = "true"
            },
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Verifies that direct HTTP JSON requests return forbidden when messaging authorization requirements are not satisfied.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenAuthorizationRequirementsAreNotSatisfied_ShouldReturnForbidden()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddRaycynixMessaging(configuration)
            .AddRequestHandler<CreateOrderRequest, CreateOrderResponse, SecuredCreateOrderRequestHandler>("/api/orders/secured")
            .AddHttpJson(options =>
            {
                options.BaseAddress = "https://orders.service.local";
                options.TimeoutSeconds = 15;
            });

        await using var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<IHttpJsonRequestProcessor>();

        var response = await processor.ProcessAsync<CreateOrderRequest, CreateOrderResponse>(
            "/api/orders/secured",
            System.Text.Encoding.UTF8.GetBytes("{\"orderId\":\"order-11\"}"),
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["X-Message-Source"] = "orders.service",
                ["X-Subject-Authenticated"] = "true",
                ["X-Subject-Id"] = "svc-orders",
                ["X-Subject-Type"] = "Service",
                ["X-Subject-Permissions"] = "orders.read"
            },
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    private sealed record CreateOrderRequest(string OrderId);

    private sealed record CreateOrderResponse(string OrderId);

    private sealed class CreateOrderRequestHandler : IRequestHandler<CreateOrderRequest, CreateOrderResponse>
    {
        public ValueTask<ResponseEnvelope<CreateOrderResponse>> HandleAsync(
            RequestEnvelope<CreateOrderRequest> request,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(new ResponseEnvelope<CreateOrderResponse>
            {
                Response = new CreateOrderResponse(request.Request.OrderId),
                CorrelationId = request.CorrelationId
            });
        }
    }

    /// <summary>
    /// Requires a stronger permission than the caller provides.
    /// </summary>
    [RequireAuthenticatedSubject]
    [RequireSubjectType(SecuritySubjectType.Service)]
    [RequirePermission("orders.write")]
    [RequireTrustedSource("orders.service")]
    private sealed class SecuredCreateOrderRequestHandler : IRequestHandler<CreateOrderRequest, CreateOrderResponse>
    {
        public ValueTask<ResponseEnvelope<CreateOrderResponse>> HandleAsync(
            RequestEnvelope<CreateOrderRequest> request,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(new ResponseEnvelope<CreateOrderResponse>
            {
                Response = new CreateOrderResponse(request.Request.OrderId),
                CorrelationId = request.CorrelationId
            });
        }
    }

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

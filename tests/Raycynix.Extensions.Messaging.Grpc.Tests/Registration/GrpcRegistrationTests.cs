using FluentAssertions;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Messaging.Abstractions.Attributes;
using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Grpc.Configurations;
using Raycynix.Extensions.Messaging.Grpc.Interfaces;
using Raycynix.Extensions.Messaging.Grpc.Internal;
using Raycynix.Extensions.Security.Abstractions.Attributes;
using Raycynix.Extensions.Security.Abstractions.Enums;

namespace Raycynix.Extensions.Messaging.Grpc.Tests.Registration;

/// <summary>
/// Covers service registration and direct request behavior for the Raycynix gRPC messaging package.
/// </summary>
public sealed class GrpcRegistrationTests
{
    /// <summary>
    /// Verifies that gRPC transport registration replaces the default direct client and registers transport options.
    /// </summary>
    [Fact]
    public void AddGrpc_ShouldRegisterGrpcConfigurationAndDirectClient()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddRaycynixMessaging(configuration)
            .AddGrpc(options => options.Address = "https://orders.grpc.local");

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<GrpcDirectMessagingConfiguration>().Address.Should().Be("https://orders.grpc.local");
        provider.GetRequiredService<IDirectRequestClient>().Should().BeOfType<GrpcRequestClient>();
    }

    /// <summary>
    /// Verifies that gRPC transport options can be bound from configuration using the default section name.
    /// </summary>
    [Fact]
    public void AddGrpc_WithConfiguration_ShouldBindConfigurationAndRegisterDirectClient()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GrpcDirectMessagingConfiguration:Address"] = "https://catalog.grpc.local"
            })
            .Build();

        services.AddRaycynixMessaging(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build())
            .AddGrpc(configuration);

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<GrpcDirectMessagingConfiguration>().Address.Should().Be("https://catalog.grpc.local");
        provider.GetRequiredService<IDirectRequestClient>().Should().BeOfType<GrpcRequestClient>();
    }

    /// <summary>
    /// Verifies that gRPC direct requests resolve the registered unary operation and return its response.
    /// </summary>
    [Fact]
    public async Task SendAsync_ShouldInvokeRegisteredGrpcUnaryOperation()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var fakeFactory = new FakeGrpcClientFactory();

        services.AddRaycynixMessaging(configuration)
            .AddGrpc(options => options.Address = "https://orders.grpc.local")
            .AddGrpcUnary<FakeOrdersGrpcClient, GetOrderRequest, GetOrderResponse>(
                "orders.v1/get",
                (client, request, cancellationToken) => client.GetAsync(request, cancellationToken));

        services.Replace(ServiceDescriptor.Singleton<IGrpcClientFactory>(fakeFactory));

        await using var provider = services.BuildServiceProvider();
        var envelopeFactory = provider.GetRequiredService<IRequestEnvelopeFactory>();
        var client = provider.GetRequiredService<IDirectRequestClient>();
        var request = envelopeFactory.Create(
            new GetOrderRequest("order-1"),
            "orders.v1/get",
            MessageFormat.Grpc,
            correlationId: "corr-2");

        var response = await client.SendAsync<GetOrderRequest, GetOrderResponse>(
            request,
            TestContext.Current.CancellationToken);

        fakeFactory.Address.Should().Be("https://orders.grpc.local");
        fakeFactory.Client.Requests.Should().ContainSingle().Which.OrderId.Should().Be("order-1");
        response.Response.OrderId.Should().Be("order-1");
        response.CorrelationId.Should().Be("corr-2");
    }

    /// <summary>
    /// Verifies that inbound gRPC requests are dispatched to the registered request handler.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_ShouldDispatchInboundGrpcRequest()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddRaycynixMessaging(configuration)
            .AddRequestHandler<GetOrderRequest, GetOrderResponse, GetOrderRequestHandler>("orders.v1/get")
            .AddGrpc(options => options.Address = "https://orders.grpc.local");

        await using var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<IGrpcRequestProcessor>();

        var response = await processor.ProcessAsync<GetOrderRequest, GetOrderResponse>(
            "orders.v1/get",
            new GetOrderRequest("order-7"),
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["X-Correlation-Id"] = "corr-7"
            },
            TestContext.Current.CancellationToken);

        response.OrderId.Should().Be("order-7");
    }

    /// <summary>
    /// Verifies that inbound gRPC requests surface an unimplemented status when no request handler is registered.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WithoutRegisteredRequestHandler_ShouldThrowRpcException()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddRaycynixMessaging(configuration)
            .AddGrpc(options => options.Address = "https://orders.grpc.local");

        await using var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<IGrpcRequestProcessor>();

        var act = () => processor.ProcessAsync<GetOrderRequest, GetOrderResponse>(
            "orders.v1/get",
            new GetOrderRequest("order-8"),
            cancellationToken: TestContext.Current.CancellationToken).AsTask();

        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.Unimplemented);
    }

    /// <summary>
    /// Verifies that malformed inbound security headers are rejected before gRPC request dispatch.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WithInvalidSecurityHeaders_ShouldThrowUnauthenticatedRpcException()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddRaycynixMessaging(configuration)
            .AddRequestHandler<GetOrderRequest, GetOrderResponse, GetOrderRequestHandler>("orders.v1/get")
            .AddGrpc(options => options.Address = "https://orders.grpc.local");

        await using var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<IGrpcRequestProcessor>();

        var act = () => processor.ProcessAsync<GetOrderRequest, GetOrderResponse>(
            "orders.v1/get",
            new GetOrderRequest("order-10"),
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["X-Subject-Authenticated"] = "true"
            },
            TestContext.Current.CancellationToken).AsTask();

        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.Unauthenticated);
    }

    /// <summary>
    /// Verifies that direct gRPC requests surface permission denied when messaging authorization requirements are not satisfied.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenAuthorizationRequirementsAreNotSatisfied_ShouldThrowPermissionDeniedRpcException()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddRaycynixMessaging(configuration)
            .AddRequestHandler<GetOrderRequest, GetOrderResponse, SecuredGetOrderRequestHandler>("orders.v1/secured-get")
            .AddGrpc(options => options.Address = "https://orders.grpc.local");

        await using var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<IGrpcRequestProcessor>();

        var act = () => processor.ProcessAsync<GetOrderRequest, GetOrderResponse>(
            "orders.v1/secured-get",
            new GetOrderRequest("order-12"),
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["X-Message-Source"] = "orders.service",
                ["X-Subject-Authenticated"] = "true",
                ["X-Subject-Id"] = "svc-orders",
                ["X-Subject-Type"] = "Service",
                ["X-Subject-Permissions"] = "orders.read"
            },
            TestContext.Current.CancellationToken).AsTask();

        var exception = await act.Should().ThrowAsync<RpcException>();
        exception.Which.StatusCode.Should().Be(StatusCode.PermissionDenied);
    }

    private sealed record GetOrderRequest(string OrderId);

    private sealed record GetOrderResponse(string OrderId);

    private sealed class GetOrderRequestHandler : IRequestHandler<GetOrderRequest, GetOrderResponse>
    {
        public ValueTask<ResponseEnvelope<GetOrderResponse>> HandleAsync(
            RequestEnvelope<GetOrderRequest> request,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(new ResponseEnvelope<GetOrderResponse>
            {
                Response = new GetOrderResponse(request.Request.OrderId),
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
    private sealed class SecuredGetOrderRequestHandler : IRequestHandler<GetOrderRequest, GetOrderResponse>
    {
        public ValueTask<ResponseEnvelope<GetOrderResponse>> HandleAsync(
            RequestEnvelope<GetOrderRequest> request,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(new ResponseEnvelope<GetOrderResponse>
            {
                Response = new GetOrderResponse(request.Request.OrderId),
                CorrelationId = request.CorrelationId
            });
        }
    }

    private sealed class FakeOrdersGrpcClient
    {
        public List<GetOrderRequest> Requests { get; } = [];

        public Task<GetOrderResponse> GetAsync(GetOrderRequest request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(new GetOrderResponse(request.OrderId));
        }
    }

    private sealed class FakeGrpcClientFactory : IGrpcClientFactory
    {
        public string? Address { get; private set; }

        public FakeOrdersGrpcClient Client { get; } = new();

        public IGrpcClientHandle Create(Type clientType, string address)
        {
            Address = address;
            return new FakeGrpcClientHandle(Client);
        }
    }

    private sealed class FakeGrpcClientHandle(object client) : IGrpcClientHandle
    {
        public object Client { get; } = client;

        public void Dispose()
        {
        }
    }
}

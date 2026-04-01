using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Grpc.Configurations;
using Raycynix.Extensions.Messaging.Grpc.Interfaces;
using Raycynix.Extensions.Messaging.Grpc.Internal;

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

    private sealed record GetOrderRequest(string OrderId);

    private sealed record GetOrderResponse(string OrderId);

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

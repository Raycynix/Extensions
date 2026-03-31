using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Contracts.Attributes;
using Raycynix.Extensions.Contracts.Constants;
using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Constants;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Attributes;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Security.Abstractions.Enums;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using System.Diagnostics;

namespace Raycynix.Extensions.Messaging.Tests.Registration;

/// <summary>
/// Covers transport-agnostic registration and codec behavior for the Raycynix messaging package.
/// </summary>
public sealed class MessagingRegistrationTests
{
    /// <summary>
    /// Verifies that the core messaging services are registered.
    /// </summary>
    [Fact]
    public void AddRaycynixMessaging_ShouldRegisterEnvelopeFactoryAndPublisher()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddRaycynixMessaging(configuration);

        using var provider = services.BuildServiceProvider();

        provider.GetService<IMessageEnvelopeFactory>().Should().NotBeNull();
        provider.GetService<IMessagePublisher>().Should().NotBeNull();
        provider.GetService<IMessageCodecResolver>().Should().NotBeNull();
        provider.GetService<IMessageDispatcher>().Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that a delegate-based gRPC codec can be resolved for the registered message type.
    /// </summary>
    [Fact]
    public void AddGrpcMessage_ShouldAllowResolvingGrpcCodecForSpecificType()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddRaycynixMessaging(configuration)
            .AddGrpcMessage<TestGrpcMessage>(
                message => message.Value,
                payload => new TestGrpcMessage(payload.ToArray()));

        using var provider = services.BuildServiceProvider();
        var resolver = provider.GetRequiredService<IMessageCodecResolver>();

        var codec = resolver.Resolve(typeof(TestGrpcMessage), MessageFormat.Grpc);

        codec.Should().NotBeNull();
        codec.Format.Should().Be(MessageFormat.Grpc);
    }

    /// <summary>
    /// Verifies that message envelopes include canonical contract headers and resolved contract metadata.
    /// </summary>
    [Fact]
    public void CreateMessageEnvelope_ShouldApplyContractHeadersAndMetadata()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddRaycynixMessaging(configuration);

        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IMessageEnvelopeFactory>();

        var envelope = factory.Create(new ContractMessage("hello"), "orders.created", MessageFormat.Json);

        envelope.Contract.Should().NotBeNull();
        envelope.Contract!.Name.Should().Be("orders.created");
        envelope.Contract.Version.ToString().Should().Be("2.1.0");
        envelope.Headers[ContractHeaders.ContractName].Should().Be("orders.created");
        envelope.Headers[ContractHeaders.ContractVersion].Should().Be("2.1.0");
    }

    /// <summary>
    /// Verifies that request envelopes propagate trace and security headers when context is available.
    /// </summary>
    [Fact]
    public void CreateRequestEnvelope_ShouldApplyTraceAndSecurityHeaders()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddSingleton<ISecurityContext>(new FakeSecurityContext());
        services.AddRaycynixMessaging(configuration);

        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IRequestEnvelopeFactory>();
        using var activity = new Activity("messaging-test");

        activity.Start();

        var envelope = factory.Create(
            new TraceableRequest("order-1"),
            "/orders/get",
            MessageFormat.Json,
            correlationId: "corr-3");

        envelope.Headers[MessageHeaderNames.CorrelationId].Should().Be("corr-3");
        envelope.Headers.Should().ContainKey(MessageHeaderNames.TraceParent);
        envelope.Headers[MessageHeaderNames.Authenticated].Should().Be(bool.TrueString);
        envelope.Headers[MessageHeaderNames.SubjectId].Should().Be("svc-orders");
        envelope.Headers[MessageHeaderNames.SubjectType].Should().Be(SecuritySubjectType.Service.ToString());
        envelope.Headers[MessageHeaderNames.SubjectRoles].Should().Be("internal");
        envelope.Headers[MessageHeaderNames.SubjectPermissions].Should().Be("orders.read");
    }

    /// <summary>
    /// Verifies that the incoming dispatcher invokes all registered handlers and returns normalized dispatch metadata.
    /// </summary>
    [Fact]
    public async Task DispatchAsync_ShouldInvokeRegisteredHandlersAndReturnDispatchResult()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddSingleton<DispatchCollector>();
        services.AddRaycynixMessaging(configuration)
            .AddMessageHandler<DispatchMessage, FirstDispatchHandler>()
            .AddMessageHandler<DispatchMessage, SecondDispatchHandler>();

        await using var provider = services.BuildServiceProvider();
        var envelopeFactory = provider.GetRequiredService<IMessageEnvelopeFactory>();
        var dispatcher = provider.GetRequiredService<IMessageDispatcher>();
        var collector = provider.GetRequiredService<DispatchCollector>();
        var envelope = envelopeFactory.Create(
            new DispatchMessage("order-1"),
            destination: "orders.created",
            format: MessageFormat.Json);

        var result = await dispatcher.DispatchAsync(envelope, TestContext.Current.CancellationToken);

        result.HandlerCount.Should().Be(2);
        result.Context.Destination.Should().Be("orders.created");
        result.Context.MessageType.Should().Be(typeof(DispatchMessage));
        collector.Values.Should().ContainInOrder("first:order-1", "second:order-1");
    }

    private sealed record TestGrpcMessage(byte[] Value);

    [MessageContract("orders.created", "2.1.0")]
    [ContractIntroduced("2.1.0")]
    private sealed record ContractMessage(string Value);

    private sealed record TraceableRequest(string OrderId);

    private sealed record DispatchMessage(string OrderId);

    private sealed class FakeSecurityContext : ISecurityContext
    {
        public bool IsAuthenticated => true;

        public string SubjectId => "svc-orders";

        public SecuritySubjectType SubjectType => SecuritySubjectType.Service;

        public IReadOnlyCollection<string> Roles => ["internal"];

        public IReadOnlyCollection<string> Permissions => ["orders.read"];
    }

    private sealed class DispatchCollector
    {
        public List<string> Values { get; } = [];
    }

    /// <summary>
    /// First test handler used for dispatcher verification.
    /// </summary>
    private sealed class FirstDispatchHandler(DispatchCollector collector) : IMessageHandler<DispatchMessage>
    {
        public ValueTask HandleAsync(
            MessageEnvelope<DispatchMessage> envelope,
            CancellationToken cancellationToken = default)
        {
            collector.Values.Add($"first:{envelope.Message.OrderId}");
            return ValueTask.CompletedTask;
        }
    }

    /// <summary>
    /// Second test handler used for dispatcher verification.
    /// </summary>
    private sealed class SecondDispatchHandler(DispatchCollector collector) : IMessageHandler<DispatchMessage>
    {
        public ValueTask HandleAsync(
            MessageEnvelope<DispatchMessage> envelope,
            CancellationToken cancellationToken = default)
        {
            collector.Values.Add($"second:{envelope.Message.OrderId}");
            return ValueTask.CompletedTask;
        }
    }
}

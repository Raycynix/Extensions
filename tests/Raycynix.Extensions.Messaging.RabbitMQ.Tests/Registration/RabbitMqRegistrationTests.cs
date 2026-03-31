using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RabbitMQ.Client;
using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.RabbitMQ.Configurations;
using Raycynix.Extensions.Messaging.RabbitMQ.Internal;

namespace Raycynix.Extensions.Messaging.RabbitMQ.Tests.Registration;

/// <summary>
/// Covers service registration and publishing behavior for the Raycynix RabbitMQ messaging package.
/// </summary>
public sealed class RabbitMqRegistrationTests
{
    /// <summary>
    /// Verifies that RabbitMQ transport registration replaces the default publisher and registers transport options.
    /// </summary>
    [Fact]
    public async Task AddRabbitMq_ShouldRegisterRabbitMqConfigurationAndPublisher()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddRaycynixMessaging(configuration)
            .AddRabbitMq(options =>
            {
                options.Host = "rabbit";
                options.Queue.Name = "messages";
            });

        await using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<RabbitMqMessagingConfiguration>().Queue.Name.Should().Be("messages");
        provider.GetRequiredService<IMessagePublisher>().GetType().Name.Should().Be("RabbitMqMessagePublisher");
    }

    /// <summary>
    /// Verifies that RabbitMQ publishing initializes topology and publishes to the configured exchange.
    /// </summary>
    [Fact]
    public async Task PublishAsync_ShouldInitializeTopologyAndPublishToConfiguredExchange()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var fakeConnectionFactory = new FakeRabbitMqConnectionFactory();

        services.AddRaycynixMessaging(configuration)
            .AddRabbitMq(options =>
            {
                options.Host = "localhost";
                options.Exchange.Name = "integration.events";
                options.Queue.Name = "orders.created";
            });

        services.Replace(ServiceDescriptor.Singleton<IRabbitMqConnectionFactory>(fakeConnectionFactory));

        await using var provider = services.BuildServiceProvider();
        var envelopeFactory = provider.GetRequiredService<IMessageEnvelopeFactory>();
        var publisher = provider.GetRequiredService<IMessagePublisher>();
        var envelope = envelopeFactory.Create(new TestMessage("hello"), "orders.created", MessageFormat.Json);

        await publisher.PublishAsync(envelope, TestContext.Current.CancellationToken);

        fakeConnectionFactory.Connection.Should().NotBeNull();
        fakeConnectionFactory.Connection!.PublishedExchange.Should().Be("integration.events");
        fakeConnectionFactory.Connection.PublishedRoutingKey.Should().Be("orders.created");
        fakeConnectionFactory.Connection.TopologyExchangeDeclared.Should().BeTrue();
        fakeConnectionFactory.Connection.TopologyQueueDeclared.Should().BeTrue();
    }

    private sealed record TestMessage(string Value);

    private sealed class FakeRabbitMqConnectionFactory : IRabbitMqConnectionFactory
    {
        public FakeRabbitMqConnection? Connection { get; private set; }

        public Task<IRabbitMqConnection> CreateAsync(
            RabbitMqMessagingConfiguration configuration,
            CancellationToken cancellationToken)
        {
            Connection = new FakeRabbitMqConnection();
            return Task.FromResult<IRabbitMqConnection>(Connection);
        }
    }

    private sealed class FakeRabbitMqConnection : IRabbitMqConnection
    {
        public bool TopologyExchangeDeclared { get; set; }

        public bool TopologyQueueDeclared { get; set; }

        public string? PublishedExchange { get; set; }

        public string? PublishedRoutingKey { get; set; }

        public Task<IRabbitMqChannel> CreateChannelAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IRabbitMqChannel>(new FakeRabbitMqChannel(this));
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FakeRabbitMqChannel(FakeRabbitMqConnection connection) : IRabbitMqChannel
    {
        public Task BasicQosAsync(uint prefetchSize, ushort prefetchCount, bool global, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task ExchangeDeclareAsync(
            string exchange,
            string type,
            bool durable,
            bool autoDelete,
            IDictionary<string, object?>? arguments,
            bool passive,
            bool noWait,
            CancellationToken cancellationToken)
        {
            connection.TopologyExchangeDeclared = true;
            return Task.CompletedTask;
        }

        public Task QueueDeclareAsync(
            string queue,
            bool durable,
            bool exclusive,
            bool autoDelete,
            IDictionary<string, object?>? arguments,
            bool passive,
            bool noWait,
            CancellationToken cancellationToken)
        {
            connection.TopologyQueueDeclared = true;
            return Task.CompletedTask;
        }

        public Task QueueBindAsync(
            string queue,
            string exchange,
            string routingKey,
            IDictionary<string, object?>? arguments,
            bool noWait,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task BasicPublishAsync(
            string exchange,
            string routingKey,
            bool mandatory,
            BasicProperties basicProperties,
            ReadOnlyMemory<byte> body,
            CancellationToken cancellationToken)
        {
            connection.PublishedExchange = exchange;
            connection.PublishedRoutingKey = routingKey;
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}

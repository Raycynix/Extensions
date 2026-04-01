using Confluent.Kafka;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Kafka.Configurations;
using Raycynix.Extensions.Messaging.Kafka.Interfaces;

namespace Raycynix.Extensions.Messaging.Kafka.Tests.Registration;

/// <summary>
/// Covers service registration and publishing behavior for the Raycynix Kafka messaging package.
/// </summary>
public sealed class KafkaRegistrationTests
{
    /// <summary>
    /// Verifies that Kafka transport registration replaces the default publisher and registers transport options.
    /// </summary>
    [Fact]
    public void AddKafka_ShouldRegisterKafkaConfigurationAndPublisher()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddRaycynixMessaging(configuration)
            .AddKafka(options =>
            {
                options.BootstrapServers = ["localhost:9092"];
                options.ClientId = "tests";
            });

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<KafkaMessagingConfiguration>().BootstrapServers.Should().ContainSingle();
        provider.GetRequiredService<IMessagePublisher>().GetType().Name.Should().Be("KafkaMessagePublisher");
    }

    /// <summary>
    /// Verifies that Kafka publishing serializes the envelope and forwards it to the configured topic.
    /// </summary>
    [Fact]
    public async Task PublishAsync_ShouldSerializeEnvelopeAndSendToKafkaTopic()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var fakeProducer = new FakeKafkaProducer();

        services.AddRaycynixMessaging(configuration)
            .AddKafka(options =>
            {
                options.BootstrapServers = ["localhost:9092"];
                options.ClientId = "tests";
            });

        services.Replace(ServiceDescriptor.Singleton<IKafkaProducer>(fakeProducer));

        await using var provider = services.BuildServiceProvider();
        var envelopeFactory = provider.GetRequiredService<IMessageEnvelopeFactory>();
        var publisher = provider.GetRequiredService<IMessagePublisher>();
        var envelope = envelopeFactory.Create(new TestMessage("hello"), "orders.created", MessageFormat.Json);

        await publisher.PublishAsync(envelope, TestContext.Current.CancellationToken);

        fakeProducer.Topic.Should().Be("orders.created");
        fakeProducer.Message.Should().NotBeNull();
        fakeProducer.Message!.Value.Should().NotBeNullOrEmpty();
        fakeProducer.Message.Headers.TryGetLastBytes("content-type", out var contentType).Should().BeTrue();
        System.Text.Encoding.UTF8.GetString(contentType!).Should().Be("application/json");
    }

    private sealed record TestMessage(string Value);

    private sealed class FakeKafkaProducer : IKafkaProducer
    {
        public string? Topic { get; private set; }

        public Message<Null, byte[]>? Message { get; private set; }

        public Task ProduceAsync(string topic, Message<Null, byte[]> message, CancellationToken cancellationToken)
        {
            Topic = topic;
            Message = message;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}

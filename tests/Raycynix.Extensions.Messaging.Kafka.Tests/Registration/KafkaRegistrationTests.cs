using Confluent.Kafka;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Messaging.Abstractions.Attributes;
using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Kafka.Configurations;
using Raycynix.Extensions.Messaging.Kafka.Interfaces;
using Raycynix.Extensions.Messaging.Kafka.Internal;

namespace Raycynix.Extensions.Messaging.Kafka.Tests.Registration;

/// <summary>
/// Covers service registration and publishing behavior for the Raycynix Kafka messaging package.
/// </summary>
public sealed class KafkaRegistrationTests
{
    /// <summary>
    /// Verifies that Kafka transport registration registers transport options and the Kafka transport publisher.
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
        provider.GetRequiredService<IMessagePublisher>().Should().NotBeNull();
        provider.GetRequiredService<ITransportMessagePublisher>().GetType().Name.Should().Be("KafkaMessagePublisher");
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

    /// <summary>
    /// Verifies that the Kafka inbound consumer dispatches incoming messages to registered handlers and commits them.
    /// </summary>
    [Fact]
    public async Task InboundConsumer_ShouldDispatchIncomingMessageToRegisteredHandler()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var fakeConsumer = new FakeKafkaConsumer();
        var fakeTransportPublisher = new FakeTransportMessagePublisher();
        var state = new ProcessingState();

        services.AddSingleton(state);
        services.AddRaycynixMessaging(configuration)
            .AddMessageHandler<InboundKafkaMessage, RecordingInboundHandler>()
            .AddKafka(options =>
            {
                options.BootstrapServers = ["localhost:9092"];
                options.ClientId = "tests";
                options.Consumer.Enabled = true;
                options.Consumer.Topics = ["orders.created"];
                options.Consumer.PollIntervalMilliseconds = 10;
            });

        fakeConsumer.Messages.Enqueue(CreateIncomingMessage("orders.created", """{"value":"hello"}"""));
        services.Replace(ServiceDescriptor.Singleton<IKafkaConsumer>(fakeConsumer));
        services.Replace(ServiceDescriptor.Singleton<ITransportMessagePublisher>(fakeTransportPublisher));

        await using var provider = services.BuildServiceProvider();
        var hostedService = new KafkaInboundConsumer(
            provider.GetRequiredService<IKafkaConsumer>(),
            provider.GetRequiredService<ITransportMessagePublisher>(),
            provider.GetRequiredService<KafkaMessagingConfiguration>(),
            provider.GetRequiredService<IServiceScopeFactory>());

        await hostedService.StartAsync(TestContext.Current.CancellationToken);
        await state.Processed.Task.WaitAsync(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
        await WaitForAsync(() => fakeConsumer.CommittedMessages.Count == 1, TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
        await hostedService.StopAsync(TestContext.Current.CancellationToken);

        state.Values.Should().ContainSingle().Which.Should().Be("hello");
        fakeConsumer.CommittedMessages.Should().ContainSingle();
        fakeTransportPublisher.PublishedMessages.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that Kafka inbound failures are republished for retry before the original message is committed.
    /// </summary>
    [Fact]
    public async Task InboundConsumer_WhenHandlerFails_ShouldRepublishForRetry()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var fakeConsumer = new FakeKafkaConsumer();
        var fakeTransportPublisher = new FakeTransportMessagePublisher();

        services.AddRaycynixMessaging(configuration, options =>
            {
                options.DispatchRetry.Enabled = false;
            })
            .AddMessageHandler<InboundKafkaMessage, AlwaysFailingInboundHandler>()
            .AddKafka(options =>
            {
                options.BootstrapServers = ["localhost:9092"];
                options.ClientId = "tests";
                options.Consumer.Enabled = true;
                options.Consumer.Topics = ["orders.created"];
                options.Consumer.PollIntervalMilliseconds = 10;
                options.Retry.Enabled = true;
                options.Retry.MaxAttempts = 2;
                options.Retry.DelayMilliseconds = 0;
            });

        fakeConsumer.Messages.Enqueue(CreateIncomingMessage("orders.created", """{"value":"retry-me"}"""));
        services.Replace(ServiceDescriptor.Singleton<IKafkaConsumer>(fakeConsumer));
        services.Replace(ServiceDescriptor.Singleton<ITransportMessagePublisher>(fakeTransportPublisher));

        await using var provider = services.BuildServiceProvider();
        var hostedService = new KafkaInboundConsumer(
            provider.GetRequiredService<IKafkaConsumer>(),
            provider.GetRequiredService<ITransportMessagePublisher>(),
            provider.GetRequiredService<KafkaMessagingConfiguration>(),
            provider.GetRequiredService<IServiceScopeFactory>());

        await hostedService.StartAsync(TestContext.Current.CancellationToken);
        await WaitForAsync(() => fakeTransportPublisher.PublishedMessages.Count == 1, TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
        await hostedService.StopAsync(TestContext.Current.CancellationToken);

        fakeConsumer.CommittedMessages.Should().ContainSingle();
        fakeTransportPublisher.PublishedMessages.Should().ContainSingle();
        fakeTransportPublisher.PublishedMessages[0].Destination.Should().Be("orders.created");
        fakeTransportPublisher.PublishedMessages[0].Headers.Should().ContainKey("X-Delivery-Attempt");
    }

    /// <summary>
    /// Verifies that Kafka inbound failures are published to the dead-letter topic when retries are exhausted.
    /// </summary>
    [Fact]
    public async Task InboundConsumer_WhenRetriesAreExhausted_ShouldPublishToDeadLetterTopic()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var fakeConsumer = new FakeKafkaConsumer();
        var fakeTransportPublisher = new FakeTransportMessagePublisher();

        services.AddRaycynixMessaging(configuration, options =>
            {
                options.DispatchRetry.Enabled = false;
            })
            .AddMessageHandler<InboundKafkaMessage, AlwaysFailingInboundHandler>()
            .AddKafka(options =>
            {
                options.BootstrapServers = ["localhost:9092"];
                options.ClientId = "tests";
                options.Consumer.Enabled = true;
                options.Consumer.Topics = ["orders.created"];
                options.Consumer.PollIntervalMilliseconds = 10;
                options.Retry.Enabled = true;
                options.Retry.MaxAttempts = 1;
                options.DeadLetter.Enabled = true;
                options.DeadLetter.Topic = "orders.dead-letter";
            });

        fakeConsumer.Messages.Enqueue(CreateIncomingMessage(
            "orders.created",
            """{"value":"dead-letter-me"}""",
            headers: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["X-Delivery-Attempt"] = "1"
            }));
        services.Replace(ServiceDescriptor.Singleton<IKafkaConsumer>(fakeConsumer));
        services.Replace(ServiceDescriptor.Singleton<ITransportMessagePublisher>(fakeTransportPublisher));

        await using var provider = services.BuildServiceProvider();
        var hostedService = new KafkaInboundConsumer(
            provider.GetRequiredService<IKafkaConsumer>(),
            provider.GetRequiredService<ITransportMessagePublisher>(),
            provider.GetRequiredService<KafkaMessagingConfiguration>(),
            provider.GetRequiredService<IServiceScopeFactory>());

        await hostedService.StartAsync(TestContext.Current.CancellationToken);
        await WaitForAsync(() => fakeTransportPublisher.PublishedMessages.Count == 1, TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
        await hostedService.StopAsync(TestContext.Current.CancellationToken);

        fakeConsumer.CommittedMessages.Should().ContainSingle();
        fakeTransportPublisher.PublishedMessages.Should().ContainSingle();
        fakeTransportPublisher.PublishedMessages[0].Destination.Should().Be("orders.dead-letter");
    }

    private sealed record TestMessage(string Value);

    [MessageContract("orders.created", "1.0.0")]
    private sealed record InboundKafkaMessage(string Value);

    private sealed class ProcessingState
    {
        public List<string> Values { get; } = [];

        public TaskCompletionSource Processed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

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

    private sealed class FakeKafkaConsumer : IKafkaConsumer
    {
        public Queue<KafkaIncomingMessage> Messages { get; } = [];

        public List<KafkaIncomingMessage> CommittedMessages { get; } = [];

        public List<string> SubscribedTopics { get; } = [];

        public void Subscribe(IEnumerable<string> topics)
        {
            SubscribedTopics.AddRange(topics);
        }

        public Task<KafkaIncomingMessage?> ConsumeAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Messages.Count > 0 ? Messages.Dequeue() : null);
        }

        public void Commit(KafkaIncomingMessage message)
        {
            CommittedMessages.Add(message);
        }

        public void Dispose()
        {
        }
    }

    private sealed class FakeTransportMessagePublisher : ITransportMessagePublisher
    {
        public List<SerializedMessage> PublishedMessages { get; } = [];

        public ValueTask PublishAsync(SerializedMessage message, CancellationToken cancellationToken = default)
        {
            PublishedMessages.Add(message);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class RecordingInboundHandler(ProcessingState state) : IMessageHandler<InboundKafkaMessage>
    {
        public ValueTask HandleAsync(
            MessageEnvelope<InboundKafkaMessage> envelope,
            CancellationToken cancellationToken = default)
        {
            state.Values.Add(envelope.Message.Value);
            state.Processed.TrySetResult();
            return ValueTask.CompletedTask;
        }
    }

    private sealed class AlwaysFailingInboundHandler : IMessageHandler<InboundKafkaMessage>
    {
        public ValueTask HandleAsync(
            MessageEnvelope<InboundKafkaMessage> envelope,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("handler failed");
        }
    }

    private static KafkaIncomingMessage CreateIncomingMessage(
        string topic,
        string json,
        IReadOnlyDictionary<string, string>? headers = null)
    {
        var actualHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["X-Contract-Name"] = "orders.created",
            ["X-Contract-Version"] = "1.0.0",
            ["X-Message-Format"] = nameof(MessageFormat.Json)
        };

        if (headers is not null)
        {
            foreach (var header in headers)
            {
                actualHeaders[header.Key] = header.Value;
            }
        }

        return new KafkaIncomingMessage
        {
            Topic = topic,
            Payload = System.Text.Encoding.UTF8.GetBytes(json),
            MessageId = Guid.NewGuid().ToString("N"),
            CorrelationId = "corr-1",
            ContentType = "application/json",
            Timestamp = DateTimeOffset.UtcNow,
            Headers = actualHeaders
        };
    }

    private static async Task WaitForAsync(
        Func<bool> condition,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        while (!condition())
        {
            if (DateTimeOffset.UtcNow - startedAt >= timeout)
            {
                throw new TimeoutException("The expected Kafka test condition was not met in time.");
            }

            await Task.Delay(25, cancellationToken);
        }
    }
}

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Implementations;

namespace Raycynix.Extensions.Messaging.Tests.Publishing;

/// <summary>
/// Covers outbox enqueueing, dispatch, and recovery behavior for outgoing messages.
/// </summary>
public sealed class MessageOutboxBehaviorTests
{
    /// <summary>
    /// Verifies that successful publishing stores and completes an outbox entry when outbox support is enabled.
    /// </summary>
    [Fact]
    public async Task PublishAsync_WhenOutboxIsEnabledAndTransportSucceeds_ShouldMarkEntryAsDispatched()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddSingleton<FakeTransportMessagePublisher>();
        services.AddRaycynixMessaging(configuration, options =>
            {
                options.Outbox.Enabled = true;
                options.Outbox.RetryDelay = TimeSpan.Zero;
            });
        services.AddSingleton<ITransportMessagePublisher>(serviceProvider =>
            serviceProvider.GetRequiredService<FakeTransportMessagePublisher>());

        await using var provider = services.BuildServiceProvider();
        var publisher = provider.GetRequiredService<IMessagePublisher>();
        var envelopeFactory = provider.GetRequiredService<IMessageEnvelopeFactory>();
        var outboxStore = provider.GetRequiredService<IMessageOutboxStore>();
        var transportPublisher = provider.GetRequiredService<FakeTransportMessagePublisher>();
        var envelope = envelopeFactory.Create(new OutboxMessage("order-1"), "orders.created", MessageFormat.Json);

        await publisher.PublishAsync(envelope, TestContext.Current.CancellationToken);

        transportPublisher.PublishedMessages.Should().ContainSingle();
        var entry = await outboxStore.GetAsync(envelope.MessageId, TestContext.Current.CancellationToken);
        entry.Should().NotBeNull();
        entry!.Status.Should().Be(MessageOutboxStatus.Dispatched);
        entry.AttemptCount.Should().Be(1);
    }

    /// <summary>
    /// Verifies that a failed transport publish leaves the outbox entry in a recoverable failed state.
    /// </summary>
    [Fact]
    public async Task PublishAsync_WhenTransportFails_ShouldPersistFailedOutboxEntry()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddSingleton(new FakeTransportMessagePublisher { FailuresBeforeSuccess = 1 });
        services.AddRaycynixMessaging(configuration, options =>
            {
                options.Outbox.Enabled = true;
                options.Outbox.RetryDelay = TimeSpan.Zero;
            });
        services.AddSingleton<ITransportMessagePublisher>(serviceProvider =>
            serviceProvider.GetRequiredService<FakeTransportMessagePublisher>());

        await using var provider = services.BuildServiceProvider();
        var publisher = provider.GetRequiredService<IMessagePublisher>();
        var envelopeFactory = provider.GetRequiredService<IMessageEnvelopeFactory>();
        var outboxStore = provider.GetRequiredService<IMessageOutboxStore>();
        var envelope = envelopeFactory.Create(new OutboxMessage("order-2"), "orders.created", MessageFormat.Json);

        var act = () => publisher.PublishAsync(envelope, TestContext.Current.CancellationToken).AsTask();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("transport failure");

        var entry = await outboxStore.GetAsync(envelope.MessageId, TestContext.Current.CancellationToken);
        entry.Should().NotBeNull();
        entry!.Status.Should().Be(MessageOutboxStatus.Failed);
        entry.AttemptCount.Should().Be(1);
        entry.Error.Should().Be("transport failure");
    }

    /// <summary>
    /// Verifies that recovery republishes failed outbox entries and marks them as dispatched.
    /// </summary>
    [Fact]
    public async Task ProcessAvailableAsync_WhenFailedOutboxEntryExists_ShouldRepublishAndCompleteEntry()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddSingleton(new FakeTransportMessagePublisher { FailuresBeforeSuccess = 1 });
        services.AddRaycynixMessaging(configuration, options =>
            {
                options.Outbox.Enabled = true;
                options.Outbox.RetryDelay = TimeSpan.Zero;
                options.Outbox.RecoveryInterval = TimeSpan.FromMilliseconds(10);
            });
        services.AddSingleton<ITransportMessagePublisher>(serviceProvider =>
            serviceProvider.GetRequiredService<FakeTransportMessagePublisher>());

        await using var provider = services.BuildServiceProvider();
        var publisher = provider.GetRequiredService<IMessagePublisher>();
        var envelopeFactory = provider.GetRequiredService<IMessageEnvelopeFactory>();
        var outboxStore = provider.GetRequiredService<IMessageOutboxStore>();
        var recoveryProcessor = provider.GetRequiredService<MessageOutboxRecoveryProcessor>();
        var transportPublisher = provider.GetRequiredService<FakeTransportMessagePublisher>();
        var envelope = envelopeFactory.Create(new OutboxMessage("order-3"), "orders.created", MessageFormat.Json);

        await publisher.Invoking(instance => instance.PublishAsync(envelope, TestContext.Current.CancellationToken).AsTask())
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("transport failure");

        var processedCount = await recoveryProcessor.ProcessAvailableAsync(TestContext.Current.CancellationToken);

        processedCount.Should().Be(1);
        transportPublisher.PublishedMessages.Should().HaveCount(2);

        var entry = await outboxStore.GetAsync(envelope.MessageId, TestContext.Current.CancellationToken);
        entry.Should().NotBeNull();
        entry!.Status.Should().Be(MessageOutboxStatus.Dispatched);
        entry.AttemptCount.Should().Be(2);
    }

    private sealed record OutboxMessage(string OrderId);

    private sealed class FakeTransportMessagePublisher : ITransportMessagePublisher
    {
        public int FailuresBeforeSuccess { get; set; }

        public List<SerializedMessage> PublishedMessages { get; } = [];

        public ValueTask PublishAsync(SerializedMessage message, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(message);

            PublishedMessages.Add(message);
            if (FailuresBeforeSuccess > 0)
            {
                FailuresBeforeSuccess--;
                throw new InvalidOperationException("transport failure");
            }

            return ValueTask.CompletedTask;
        }
    }
}

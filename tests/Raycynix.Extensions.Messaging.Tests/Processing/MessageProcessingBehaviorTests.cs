using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Metrics.Abstractions;
using Raycynix.Extensions.Metrics.Abstractions.Interfaces;

namespace Raycynix.Extensions.Messaging.Tests.Processing;

/// <summary>
/// Covers retry, failure handling, and observability behavior for message processing.
/// </summary>
public sealed class MessageProcessingBehaviorTests
{
    /// <summary>
    /// Verifies that dispatch retries a transient handler failure and reports the successful attempt count.
    /// </summary>
    [Fact]
    public async Task DispatchAsync_WhenHandlerFailsInitially_ShouldRetryAndSucceed()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddSingleton<RetryingHandlerState>();
        services.AddRaycynixMessaging(configuration, options =>
            {
                options.DispatchRetry.Enabled = true;
                options.DispatchRetry.MaxRetries = 2;
                options.DispatchRetry.Delay = TimeSpan.Zero;
            })
            .AddMessageHandler<RetryableDispatchMessage, RetryingDispatchHandler>();

        await using var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IMessageDispatcher>();
        var envelopeFactory = provider.GetRequiredService<IMessageEnvelopeFactory>();
        var state = provider.GetRequiredService<RetryingHandlerState>();
        var envelope = envelopeFactory.Create(
            new RetryableDispatchMessage("order-1"),
            "orders.created",
            MessageFormat.Json);

        var result = await dispatcher.DispatchAsync(envelope, TestContext.Current.CancellationToken);

        result.HandlerCount.Should().Be(1);
        result.AttemptCount.Should().Be(2);
        state.AttemptCount.Should().Be(2);
        state.ProcessedOrders.Should().ContainSingle().Which.Should().Be("order-1");
    }

    /// <summary>
    /// Verifies that dispatch stops after the configured retry count when handler failures persist.
    /// </summary>
    [Fact]
    public async Task DispatchAsync_WhenHandlerKeepsFailing_ShouldThrowAfterRetriesAreExhausted()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddSingleton<AlwaysFailingHandlerState>();
        services.AddRaycynixMessaging(configuration, options =>
            {
                options.DispatchRetry.Enabled = true;
                options.DispatchRetry.MaxRetries = 2;
                options.DispatchRetry.Delay = TimeSpan.Zero;
            })
            .AddMessageHandler<RetryableDispatchMessage, AlwaysFailingDispatchHandler>();

        await using var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IMessageDispatcher>();
        var envelopeFactory = provider.GetRequiredService<IMessageEnvelopeFactory>();
        var state = provider.GetRequiredService<AlwaysFailingHandlerState>();
        var envelope = envelopeFactory.Create(
            new RetryableDispatchMessage("order-2"),
            "orders.created",
            MessageFormat.Json);

        var act = () => dispatcher.DispatchAsync(envelope, TestContext.Current.CancellationToken).AsTask();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("persistent failure");
        state.AttemptCount.Should().Be(3);
    }

    /// <summary>
    /// Verifies that dispatch records success metrics when a metrics service is available.
    /// </summary>
    [Fact]
    public async Task DispatchAsync_WhenMetricsAreAvailable_ShouldRecordDispatchMetrics()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var metrics = new FakeMetricsService();
        services.AddSingleton<IMetricsService>(metrics);
        services.AddRaycynixMessaging(configuration)
            .AddMessageHandler<ObservedDispatchMessage, ObservedDispatchHandler>();

        await using var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IMessageDispatcher>();
        var envelopeFactory = provider.GetRequiredService<IMessageEnvelopeFactory>();
        var envelope = envelopeFactory.Create(
            new ObservedDispatchMessage("order-3"),
            "orders.observed",
            MessageFormat.Json);

        var result = await dispatcher.DispatchAsync(envelope, TestContext.Current.CancellationToken);
        var expectedLabels = new[] { typeof(ObservedDispatchMessage).FullName!, "orders.observed", "success" };

        result.AttemptCount.Should().Be(1);
        metrics.Counters.Should().ContainKey("raycynix_messaging_dispatch_total");
        metrics.Histograms.Should().ContainKey("raycynix_messaging_dispatch_duration_seconds");
        metrics.Counters["raycynix_messaging_dispatch_total"].Records.Should().ContainSingle();
        metrics.Counters["raycynix_messaging_dispatch_total"].Records[0].Value.Should().Be(1);
        metrics.Counters["raycynix_messaging_dispatch_total"].Records[0].LabelValues.Should().Equal(expectedLabels);
        metrics.Histograms["raycynix_messaging_dispatch_duration_seconds"].MeasureCount.Should().Be(1);
    }

    private sealed record RetryableDispatchMessage(string OrderId);

    private sealed record ObservedDispatchMessage(string OrderId);

    private sealed class RetryingHandlerState
    {
        public int AttemptCount { get; set; }

        public List<string> ProcessedOrders { get; } = [];
    }

    private sealed class AlwaysFailingHandlerState
    {
        public int AttemptCount { get; set; }
    }

    /// <summary>
    /// Fails once to verify retry behavior.
    /// </summary>
    private sealed class RetryingDispatchHandler(RetryingHandlerState state) : IMessageHandler<RetryableDispatchMessage>
    {
        public ValueTask HandleAsync(
            MessageEnvelope<RetryableDispatchMessage> envelope,
            CancellationToken cancellationToken = default)
        {
            state.AttemptCount++;

            if (state.AttemptCount == 1)
            {
                throw new InvalidOperationException("transient failure");
            }

            state.ProcessedOrders.Add(envelope.Message.OrderId);
            return ValueTask.CompletedTask;
        }
    }

    /// <summary>
    /// Always fails to verify retry exhaustion behavior.
    /// </summary>
    private sealed class AlwaysFailingDispatchHandler(AlwaysFailingHandlerState state) : IMessageHandler<RetryableDispatchMessage>
    {
        public ValueTask HandleAsync(
            MessageEnvelope<RetryableDispatchMessage> envelope,
            CancellationToken cancellationToken = default)
        {
            state.AttemptCount++;
            throw new InvalidOperationException("persistent failure");
        }
    }

    /// <summary>
    /// Successful handler used for observability verification.
    /// </summary>
    private sealed class ObservedDispatchHandler : IMessageHandler<ObservedDispatchMessage>
    {
        public ValueTask HandleAsync(
            MessageEnvelope<ObservedDispatchMessage> envelope,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FakeMetricsService : IMetricsService
    {
        public Dictionary<string, FakeCounter> Counters { get; } = [];

        public Dictionary<string, FakeHistogram> Histograms { get; } = [];

        public IMetricCounter CreateCounter(string name, string help, params string[] labelNames)
        {
            var counter = new FakeCounter();
            Counters[name] = counter;
            return counter;
        }

        public IMetricGauge CreateGauge(string name, string help, params string[] labelNames)
        {
            return new FakeGauge();
        }

        public IMetricHistogram CreateHistogram(string name, string help, params string[] labelNames)
        {
            var histogram = new FakeHistogram();
            Histograms[name] = histogram;
            return histogram;
        }
    }

    private sealed class FakeCounter : IMetricCounter
    {
        public List<(double Value, string[] LabelValues)> Records { get; } = [];

        public void Increment(double value = 1, params string[] labelValues)
        {
            Records.Add((value, labelValues));
        }
    }

    private sealed class FakeGauge : IMetricGauge
    {
        public void Set(double value, params string[] labelValues)
        {
        }

        public void Increment(double value = 1, params string[] labelValues)
        {
        }

        public void Decrement(double value = 1, params string[] labelValues)
        {
        }
    }

    private sealed class FakeHistogram : IMetricHistogram
    {
        public int MeasureCount { get; private set; }

        public void Observe(double value, params string[] labelValues)
        {
        }

        public IDisposable MeasureDuration(params string[] labelValues)
        {
            MeasureCount++;
            return NoopDisposable.Instance;
        }
    }

    private sealed class NoopDisposable : IDisposable
    {
        public static NoopDisposable Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}

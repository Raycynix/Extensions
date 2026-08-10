using System.Diagnostics.Metrics;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Messaging.Abstractions.Attributes;
using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Exceptions;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Metrics.Abstractions;
using Raycynix.Extensions.Security.Abstractions.Attributes;
using Raycynix.Extensions.Security.Abstractions.Enums;
using Raycynix.Extensions.Security.Abstractions.Interfaces;

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

        using var metrics = new MetricRecorder();
        services.AddSingleton<IMeterFactory>(metrics);
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
        result.AttemptCount.Should().Be(1);
        metrics.LongMeasurements.Should().ContainSingle(measurement =>
            measurement.InstrumentName == "raycynix.messaging.dispatches" &&
            measurement.Value == 1 &&
            HasTag(measurement.Tags, "raycynix.messaging.message.type", typeof(ObservedDispatchMessage).FullName!) &&
            HasTag(measurement.Tags, "raycynix.messaging.destination", "orders.observed") &&
            HasTag(measurement.Tags, "raycynix.messaging.status", "success"));
        metrics.DoubleMeasurements.Should().ContainSingle(measurement =>
            measurement.InstrumentName == "raycynix.messaging.dispatch.duration");
    }

    /// <summary>
    /// Verifies that direct request dispatch records success metrics when a metrics service is available.
    /// </summary>
    [Fact]
    public async Task DispatchRequestAsync_WhenMetricsAreAvailable_ShouldRecordRequestMetrics()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        using var metrics = new MetricRecorder();
        services.AddSingleton<IMeterFactory>(metrics);
        services.AddRaycynixMessaging(configuration)
            .AddRequestHandler<ObservedRequest, ObservedResponse, ObservedRequestHandler>("orders.v1/get");

        await using var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IRequestDispatcher>();
        var envelopeFactory = provider.GetRequiredService<IRequestEnvelopeFactory>();
        var request = envelopeFactory.Create(
            new ObservedRequest("order-4"),
            "orders.v1/get",
            MessageFormat.Json);

        var response = await dispatcher.DispatchAsync<ObservedRequest, ObservedResponse>(
            request,
            TestContext.Current.CancellationToken);
        response.Response.OrderId.Should().Be("order-4");
        metrics.LongMeasurements.Should().ContainSingle(measurement =>
            measurement.InstrumentName == "raycynix.messaging.requests" &&
            measurement.Value == 1 &&
            HasTag(measurement.Tags, "raycynix.messaging.request.type", typeof(ObservedRequest).FullName!) &&
            HasTag(measurement.Tags, "raycynix.messaging.destination", "orders.v1/get") &&
            HasTag(measurement.Tags, "raycynix.messaging.status", "success"));
        metrics.DoubleMeasurements.Should().ContainSingle(measurement =>
            measurement.InstrumentName == "raycynix.messaging.request.duration");
    }

    /// <summary>
    /// Verifies that an already processed incoming message is skipped on subsequent deliveries.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenMessageWasAlreadyProcessed_ShouldSkipDuplicateDelivery()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddSingleton<IncomingProcessingState>();
        services.AddRaycynixMessaging(configuration)
            .AddMessageHandler<IncomingProcessingMessage, RecordingIncomingHandler>();

        await using var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<IIncomingMessageProcessor>();
        var inboxStore = provider.GetRequiredService<IIncomingMessageInboxStore>();
        var state = provider.GetRequiredService<IncomingProcessingState>();
        var message = CreateIncomingMessage("msg-1", """{"value":"once"}""");

        await processor.ProcessAsync(message, TestContext.Current.CancellationToken);
        await processor.ProcessAsync(message, TestContext.Current.CancellationToken);

        state.Values.Should().ContainSingle().Which.Should().Be("once");

        var inboxEntry = await inboxStore.GetAsync("msg-1", TestContext.Current.CancellationToken);
        inboxEntry.Should().NotBeNull();
        inboxEntry!.Status.Should().Be(IncomingMessageInboxStatus.Processed);
    }

    /// <summary>
    /// Verifies that a failed incoming message can be processed successfully on a later retry.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenPreviousAttemptFailed_ShouldAllowRetry()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddSingleton<RetryingIncomingState>();
        services.AddRaycynixMessaging(configuration)
            .AddMessageHandler<IncomingProcessingMessage, RetryingIncomingHandler>();

        await using var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<IIncomingMessageProcessor>();
        var inboxStore = provider.GetRequiredService<IIncomingMessageInboxStore>();
        var state = provider.GetRequiredService<RetryingIncomingState>();
        var message = CreateIncomingMessage("msg-2", """{"value":"retry"}""");

        var firstAttempt = () => processor.ProcessAsync(message, TestContext.Current.CancellationToken);
        await firstAttempt.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("incoming failure");

        await processor.ProcessAsync(message, TestContext.Current.CancellationToken);

        state.AttemptCount.Should().Be(2);
        state.Values.Should().ContainSingle().Which.Should().Be("retry");

        var inboxEntry = await inboxStore.GetAsync("msg-2", TestContext.Current.CancellationToken);
        inboxEntry.Should().NotBeNull();
        inboxEntry!.Status.Should().Be(IncomingMessageInboxStatus.Processed);
    }

    /// <summary>
    /// Verifies that inbound handlers receive a scoped security context and execute when source and authorization requirements are satisfied.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenInboundAuthorizationRequirementsAreSatisfied_ShouldPopulateSecurityContextAndExecuteHandler()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MessagingConfiguration:SourceName"] = "tests.service"
            })
            .Build();

        services.AddSingleton<SecuredIncomingState>();
        services.AddRaycynixMessaging(configuration, options =>
            {
                options.IncomingProcessing.TrustedSources.Add("orders.service");
            })
            .AddMessageHandler<IncomingProcessingMessage, SecuredIncomingHandler>();

        await using var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<IIncomingMessageProcessor>();
        var message = CreateIncomingMessage("msg-3", """{"value":"secured"}""", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["X-Message-Source"] = "orders.service",
            ["X-Subject-Authenticated"] = "true",
            ["X-Subject-Id"] = "svc-orders",
            ["X-Subject-Type"] = "Service",
            ["X-Subject-Permissions"] = "orders.read"
        });

        await processor.ProcessAsync(message, TestContext.Current.CancellationToken);

        provider.GetRequiredService<SecuredIncomingState>().Records.Should().ContainSingle();
        provider.GetRequiredService<SecuredIncomingState>().Records[0].Should().Be(("secured", "svc-orders", SecuritySubjectType.Service));
    }

    /// <summary>
    /// Verifies that inbound handlers fail fast when declarative authorization requirements are not satisfied.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenInboundAuthorizationRequirementsAreNotSatisfied_ShouldThrowAuthorizationException()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddSingleton<SecuredIncomingState>();
        services.AddRaycynixMessaging(configuration)
            .AddMessageHandler<IncomingProcessingMessage, SecuredIncomingHandler>();

        await using var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<IIncomingMessageProcessor>();
        var message = CreateIncomingMessage("msg-4", """{"value":"denied"}""", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["X-Message-Source"] = "orders.service",
            ["X-Subject-Authenticated"] = "true",
            ["X-Subject-Id"] = "svc-orders",
            ["X-Subject-Type"] = "Service",
            ["X-Subject-Permissions"] = "orders.write"
        });

        var act = () => processor.ProcessAsync(message, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<IncomingMessageAuthorizationException>();
    }

    /// <summary>
    /// Verifies that inbound processing rejects messages from untrusted sources when trusted sources are configured.
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WhenInboundSourceIsUntrusted_ShouldThrowAuthenticationException()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddRaycynixMessaging(configuration, options =>
            {
                options.IncomingProcessing.TrustedSources.Add("orders.service");
            })
            .AddMessageHandler<IncomingProcessingMessage, RecordingIncomingHandler>();

        await using var provider = services.BuildServiceProvider();
        var processor = provider.GetRequiredService<IIncomingMessageProcessor>();
        var message = CreateIncomingMessage("msg-5", """{"value":"untrusted"}""", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["X-Message-Source"] = "billing.service"
        });

        var act = () => processor.ProcessAsync(message, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<IncomingMessageAuthenticationException>();
    }

    private sealed record RetryableDispatchMessage(string OrderId);

    private sealed record ObservedDispatchMessage(string OrderId);

    private sealed record ObservedRequest(string OrderId);

    private sealed record ObservedResponse(string OrderId);

    private sealed record IncomingProcessingMessage(string Value);

    private sealed class RetryingHandlerState
    {
        public int AttemptCount { get; set; }

        public List<string> ProcessedOrders { get; } = [];
    }

    private sealed class AlwaysFailingHandlerState
    {
        public int AttemptCount { get; set; }
    }

    private sealed class IncomingProcessingState
    {
        public List<string> Values { get; } = [];
    }

    private sealed class RetryingIncomingState
    {
        public int AttemptCount { get; set; }

        public List<string> Values { get; } = [];
    }

    private sealed class SecuredIncomingState
    {
        public List<(string Value, string SubjectId, SecuritySubjectType SubjectType)> Records { get; } = [];
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

    /// <summary>
    /// Successful request handler used for request observability verification.
    /// </summary>
    private sealed class ObservedRequestHandler : IRequestHandler<ObservedRequest, ObservedResponse>
    {
        public ValueTask<ResponseEnvelope<ObservedResponse>> HandleAsync(
            RequestEnvelope<ObservedRequest> request,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(new ResponseEnvelope<ObservedResponse>
            {
                Response = new ObservedResponse(request.Request.OrderId),
                CorrelationId = request.CorrelationId
            });
        }
    }

    /// <summary>
    /// Records incoming payload values for duplicate-delivery verification.
    /// </summary>
    private sealed class RecordingIncomingHandler(IncomingProcessingState state) : IMessageHandler<IncomingProcessingMessage>
    {
        public ValueTask HandleAsync(
            MessageEnvelope<IncomingProcessingMessage> envelope,
            CancellationToken cancellationToken = default)
        {
            state.Values.Add(envelope.Message.Value);
            return ValueTask.CompletedTask;
        }
    }

    /// <summary>
    /// Fails once and then succeeds for inbox retry verification.
    /// </summary>
    private sealed class RetryingIncomingHandler(RetryingIncomingState state) : IMessageHandler<IncomingProcessingMessage>
    {
        public ValueTask HandleAsync(
            MessageEnvelope<IncomingProcessingMessage> envelope,
            CancellationToken cancellationToken = default)
        {
            state.AttemptCount++;

            if (state.AttemptCount == 1)
            {
                throw new InvalidOperationException("incoming failure");
            }

            state.Values.Add(envelope.Message.Value);
            return ValueTask.CompletedTask;
        }
    }

    /// <summary>
    /// Requires source trust and permission-based authorization for inbound execution.
    /// </summary>
    [RequireAuthenticatedSubject]
    [RequireSubjectType(SecuritySubjectType.Service)]
    [RequirePermission("orders.read")]
    [RequireTrustedSource("orders.service")]
    private sealed class SecuredIncomingHandler(
        SecuredIncomingState state,
        ISecurityContext securityContext) : IMessageHandler<IncomingProcessingMessage>
    {
        public ValueTask HandleAsync(
            MessageEnvelope<IncomingProcessingMessage> envelope,
            CancellationToken cancellationToken = default)
        {
            state.Records.Add((envelope.Message.Value, securityContext.SubjectId, securityContext.SubjectType));
            return ValueTask.CompletedTask;
        }
    }

    private static bool HasTag(
        IReadOnlyCollection<KeyValuePair<string, object?>> tags,
        string key,
        object value)
    {
        return tags.Any(tag => tag.Key == key && Equals(tag.Value, value));
    }

    private sealed class MetricRecorder : IMeterFactory, IDisposable
    {
        private readonly MeterListener _listener;
        private readonly List<Meter> _meters = [];

        public MetricRecorder()
        {
            _listener = new MeterListener
            {
                InstrumentPublished = (instrument, listener) =>
                {
                    if (instrument.Meter.Name == RaycynixMetrics.MeterName)
                    {
                        listener.EnableMeasurementEvents(instrument);
                    }
                }
            };
            _listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
                LongMeasurements.Add(new(instrument.Name, value, tags.ToArray())));
            _listener.SetMeasurementEventCallback<double>((instrument, value, tags, _) =>
                DoubleMeasurements.Add(new(instrument.Name, value, tags.ToArray())));
            _listener.Start();
        }

        public List<Measurement<long>> LongMeasurements { get; } = [];
        public List<Measurement<double>> DoubleMeasurements { get; } = [];

        public Meter Create(MeterOptions options)
        {
            var meter = new Meter(options);
            _meters.Add(meter);
            return meter;
        }

        public void Dispose()
        {
            _listener.Dispose();
            foreach (var meter in _meters)
            {
                meter.Dispose();
            }
        }
    }

    private sealed record Measurement<T>(
        string InstrumentName,
        T Value,
        KeyValuePair<string, object?>[] Tags);

    private static IncomingTransportMessage CreateIncomingMessage(
        string messageId,
        string json,
        IReadOnlyDictionary<string, string>? headers = null)
    {
        var actualHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["X-Message-Format"] = MessageFormat.Json.ToString()
        };

        if (headers is not null)
        {
            foreach (var header in headers)
            {
                actualHeaders[header.Key] = header.Value;
            }
        }

        return new IncomingTransportMessage
        {
            Destination = "raycynix.extensions.messaging.tests.processing.incoming-processing-message",
            Payload = System.Text.Encoding.UTF8.GetBytes(json),
            Format = MessageFormat.Json,
            MessageId = messageId,
            CreatedAt = DateTimeOffset.UtcNow,
            Headers = actualHeaders
        };
    }
}

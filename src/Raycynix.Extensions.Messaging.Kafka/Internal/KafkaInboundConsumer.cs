using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Messaging.Abstractions.Constants;
using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Kafka.Configurations;
using Raycynix.Extensions.Messaging.Kafka.Interfaces;

namespace Raycynix.Extensions.Messaging.Kafka.Internal;

/// <summary>
/// Polls Kafka for inbound messages and dispatches them to registered message handlers.
/// </summary>
internal sealed class KafkaInboundConsumer(
    IKafkaConsumer consumer,
    ITransportMessagePublisher transportPublisher,
    KafkaMessagingConfiguration configuration,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<KafkaInboundConsumer>? logger = null) : BackgroundService
{
    private const string DeliveryAttemptHeader = "X-Delivery-Attempt";
    private const string ErrorHeader = "X-Processing-Error";
    private const string OriginalTopicHeader = "X-Original-Topic";

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!configuration.Consumer.Enabled)
        {
            logger?.LogDebug("Kafka inbound consumer is disabled.");
            return;
        }

        logger?.LogInformation("Starting Kafka inbound consumer. TopicCount={TopicCount}.",
            configuration.Consumer.Topics.Count());
        consumer.Subscribe(configuration.Consumer.Topics);

        while (!stoppingToken.IsCancellationRequested)
        {
            var message = await consumer.ConsumeAsync(stoppingToken).ConfigureAwait(false);
            if (message is null)
            {
                await DelayWhenIdleAsync(stoppingToken).ConfigureAwait(false);
                continue;
            }

            try
            {
                await using var scope = serviceScopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<IIncomingMessageProcessor>();
                logger?.LogDebug(
                    "Processing Kafka message. Topic={Topic}, HeaderCount={HeaderCount}.",
                    message.Topic,
                    message.Headers.Count);
                await processor.ProcessAsync(CreateIncomingMessage(message), stoppingToken).ConfigureAwait(false);
                consumer.Commit(message);
                logger?.LogDebug("Kafka message committed. Topic={Topic}.", message.Topic);
            }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                logger?.LogWarning(exception, "Kafka message processing failed. Topic={Topic}.", message.Topic);
                await HandleFailureAsync(message, exception, stoppingToken).ConfigureAwait(false);
                consumer.Commit(message);
            }
        }
    }

    private IncomingTransportMessage CreateIncomingMessage(KafkaIncomingMessage message)
    {
        var headers = new Dictionary<string, string>(message.Headers, StringComparer.OrdinalIgnoreCase);
        var format = ResolveFormat(headers, message.ContentType);

        return new IncomingTransportMessage
        {
            Destination = message.Topic,
            Payload = message.Payload,
            Format = format,
            ContentType = message.ContentType,
            MessageId = message.MessageId,
            CorrelationId = message.CorrelationId,
            CausationId = message.CausationId,
            CreatedAt = message.Timestamp ?? DateTimeOffset.UtcNow,
            Headers = headers
        };
    }

    private async Task HandleFailureAsync(
        KafkaIncomingMessage message,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var attempt = GetDeliveryAttempt(message.Headers);

        if (configuration.Retry.Enabled && attempt < configuration.Retry.MaxAttempts)
        {
            logger?.LogDebug(
                "Republishing Kafka message for retry. Topic={Topic}, Attempt={Attempt}, MaxAttempts={MaxAttempts}.",
                message.Topic,
                attempt + 1,
                configuration.Retry.MaxAttempts);

            if (configuration.Retry.DelayMilliseconds > 0)
            {
                await Task.Delay(configuration.Retry.DelayMilliseconds, cancellationToken).ConfigureAwait(false);
            }

            await transportPublisher.PublishAsync(
                    CreateRetryMessage(message, message.Topic, attempt + 1, exception),
                    cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        if (configuration.DeadLetter.Enabled)
        {
            logger?.LogWarning(
                exception,
                "Publishing Kafka message to dead-letter topic. Topic={Topic}, DeadLetterTopic={DeadLetterTopic}, Attempt={Attempt}.",
                message.Topic,
                configuration.DeadLetter.Topic,
                attempt);

            await transportPublisher.PublishAsync(
                    CreateRetryMessage(message, configuration.DeadLetter.Topic, attempt, exception),
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private static SerializedMessage CreateRetryMessage(
        KafkaIncomingMessage message,
        string destination,
        int nextAttempt,
        Exception exception)
    {
        var headers = new Dictionary<string, string>(message.Headers, StringComparer.OrdinalIgnoreCase)
        {
            [DeliveryAttemptHeader] = nextAttempt.ToString(),
            [ErrorHeader] = exception.GetType().Name,
            [OriginalTopicHeader] = message.Topic
        };

        return new SerializedMessage
        {
            Destination = destination,
            Payload = message.Payload,
            Format = ResolveFormat(headers, message.ContentType),
            ContentType = message.ContentType ?? "application/json",
            MessageId = message.MessageId,
            CorrelationId = message.CorrelationId,
            CausationId = message.CausationId,
            CreatedAt = message.Timestamp ?? DateTimeOffset.UtcNow,
            Headers = headers
        };
    }

    private static int GetDeliveryAttempt(IReadOnlyDictionary<string, string> headers)
    {
        return headers.TryGetValue(DeliveryAttemptHeader, out var value) && int.TryParse(value, out var attempt) &&
               attempt > 0
            ? attempt
            : 1;
    }

    private static MessageFormat ResolveFormat(IReadOnlyDictionary<string, string> headers, string? contentType)
    {
        if (headers.TryGetValue(MessageHeaderNames.Format, out var formatValue) &&
            Enum.TryParse<MessageFormat>(formatValue, ignoreCase: true, out var format))
        {
            return format;
        }

        if (!string.IsNullOrWhiteSpace(contentType) &&
            contentType.Contains("grpc", StringComparison.OrdinalIgnoreCase))
        {
            return MessageFormat.Grpc;
        }

        return MessageFormat.Json;
    }

    private async Task DelayWhenIdleAsync(CancellationToken cancellationToken)
    {
        if (configuration.Consumer.PollIntervalMilliseconds <= 0)
        {
            await Task.Yield();
            return;
        }

        await Task.Delay(configuration.Consumer.PollIntervalMilliseconds, cancellationToken).ConfigureAwait(false);
    }
}

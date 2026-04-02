using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
    IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private const string DeliveryAttemptHeader = "X-Delivery-Attempt";
    private const string ErrorHeader = "X-Processing-Error";
    private const string OriginalTopicHeader = "X-Original-Topic";

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!configuration.Consumer.Enabled)
        {
            return;
        }

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
                await processor.ProcessAsync(CreateIncomingMessage(message), stoppingToken).ConfigureAwait(false);
                consumer.Commit(message);
            }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                await HandleFailureAsync(message, exception, CancellationToken.None).ConfigureAwait(false);
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
            [ErrorHeader] = exception.Message,
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
        return headers.TryGetValue(DeliveryAttemptHeader, out var value) && int.TryParse(value, out var attempt) && attempt > 0
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

using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Messaging.Abstractions.Constants;
using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.RabbitMQ.Configurations;
using RabbitMQ.Client;
using Raycynix.Extensions.Messaging.RabbitMQ.Interfaces;

namespace Raycynix.Extensions.Messaging.RabbitMQ.Internal;

/// <summary>
/// Polls RabbitMQ for inbound deliveries and dispatches them to registered message handlers.
/// </summary>
internal sealed class RabbitMqInboundConsumer(
    RabbitMqConnectionAccessor connectionAccessor,
    RabbitMqMessagingConfiguration configuration,
    IIncomingMessageProcessor processor) : BackgroundService
{
    private const string DeliveryAttemptHeader = "X-Delivery-Attempt";
    private const string ErrorHeader = "X-Processing-Error";
    private const string OriginalRoutingKeyHeader = "X-Original-Routing-Key";

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!configuration.Consumer.Enabled)
        {
            return;
        }

        await using var channel = await connectionAccessor.CreateChannelAsync(stoppingToken).ConfigureAwait(false);

        while (!stoppingToken.IsCancellationRequested)
        {
            var delivery = await channel.BasicGetAsync(configuration.Queue.Name, autoAck: false, stoppingToken).ConfigureAwait(false);
            if (delivery is null)
            {
                await DelayWhenIdleAsync(stoppingToken).ConfigureAwait(false);
                continue;
            }

            try
            {
                var incomingMessage = CreateIncomingMessage(delivery);
                await processor.ProcessAsync(incomingMessage, stoppingToken).ConfigureAwait(false);
                await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, cancellationToken: CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                await HandleFailureAsync(channel, delivery, exception, CancellationToken.None).ConfigureAwait(false);
            }
        }
    }

    private IncomingTransportMessage CreateIncomingMessage(RabbitMqIncomingDelivery delivery)
    {
        var headers = new Dictionary<string, string>(delivery.Headers, StringComparer.OrdinalIgnoreCase);
        var format = ResolveFormat(headers, delivery.ContentType);

        return new IncomingTransportMessage
        {
            Destination = delivery.RoutingKey,
            Payload = delivery.Body,
            Format = format,
            ContentType = delivery.ContentType,
            MessageId = delivery.MessageId ?? Guid.NewGuid().ToString("N"),
            CorrelationId = delivery.CorrelationId,
            CausationId = headers.TryGetValue("causation-id", out var causationId) ? causationId : null,
            CreatedAt = delivery.Timestamp ?? DateTimeOffset.UtcNow,
            Headers = headers
        };
    }

    private async Task HandleFailureAsync(
        IRabbitMqChannel channel,
        RabbitMqIncomingDelivery delivery,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var attempt = GetDeliveryAttempt(delivery.Headers);

        if (configuration.Retry.Enabled && attempt < configuration.Retry.MaxAttempts)
        {
            if (configuration.Retry.DelayMilliseconds > 0)
            {
                await Task.Delay(configuration.Retry.DelayMilliseconds, cancellationToken).ConfigureAwait(false);
            }

            await RepublishAsync(
                    channel,
                    exchange: configuration.Exchange.Name,
                    routingKey: delivery.RoutingKey,
                    delivery,
                    nextAttempt: attempt + 1,
                    exception,
                    cancellationToken)
                .ConfigureAwait(false);

            await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, cancellationToken: cancellationToken).ConfigureAwait(false);
            return;
        }

        if (configuration.DeadLetter.Enabled)
        {
            await RepublishAsync(
                    channel,
                    exchange: configuration.DeadLetter.Exchange,
                    routingKey: configuration.DeadLetter.RoutingKey,
                    delivery,
                    nextAttempt: attempt,
                    exception,
                    cancellationToken)
                .ConfigureAwait(false);

            await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, cancellationToken: cancellationToken).ConfigureAwait(false);
            return;
        }

        await channel.BasicRejectAsync(delivery.DeliveryTag, requeue: false, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private async Task RepublishAsync(
        IRabbitMqChannel channel,
        string exchange,
        string routingKey,
        RabbitMqIncomingDelivery delivery,
        int nextAttempt,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var headers = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var header in delivery.Headers)
        {
            headers[header.Key] = System.Text.Encoding.UTF8.GetBytes(header.Value);
        }

        headers[DeliveryAttemptHeader] = System.Text.Encoding.UTF8.GetBytes(nextAttempt.ToString());
        headers[ErrorHeader] = System.Text.Encoding.UTF8.GetBytes(exception.Message);
        headers[OriginalRoutingKeyHeader] = System.Text.Encoding.UTF8.GetBytes(delivery.RoutingKey);

        var properties = new BasicProperties
        {
            MessageId = delivery.MessageId,
            CorrelationId = delivery.CorrelationId,
            ContentType = delivery.ContentType,
            Timestamp = new AmqpTimestamp((delivery.Timestamp ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds()),
            Headers = headers
        };

        await channel.BasicPublishAsync(
                exchange,
                routingKey,
                mandatory: false,
                properties,
                delivery.Body,
                cancellationToken)
            .ConfigureAwait(false);
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

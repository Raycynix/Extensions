using RabbitMQ.Client;
using Raycynix.Extensions.Messaging.RabbitMQ.Interfaces;

namespace Raycynix.Extensions.Messaging.RabbitMQ.Internal;

/// <summary>
/// RabbitMQ channel adapter over the client library.
/// </summary>
internal sealed class RabbitMqChannel(IChannel channel) : IRabbitMqChannel
{
    /// <inheritdoc />
    public Task BasicQosAsync(uint prefetchSize, ushort prefetchCount, bool global, CancellationToken cancellationToken)
    {
        return channel.BasicQosAsync(prefetchSize, prefetchCount, global, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<RabbitMqIncomingDelivery?> BasicGetAsync(
        string queue,
        bool autoAck,
        CancellationToken cancellationToken)
    {
        var result = await channel.BasicGetAsync(queue, autoAck, cancellationToken).ConfigureAwait(false);
        if (result is null)
        {
            return null;
        }

        return new RabbitMqIncomingDelivery
        {
            DeliveryTag = result.DeliveryTag,
            RoutingKey = result.RoutingKey,
            Body = result.Body,
            MessageId = result.BasicProperties.MessageId,
            CorrelationId = result.BasicProperties.CorrelationId,
            ContentType = result.BasicProperties.ContentType,
            Timestamp = result.BasicProperties.Timestamp.UnixTime is { } unixTime && unixTime > 0
                ? DateTimeOffset.FromUnixTimeSeconds(unixTime)
                : null,
            Headers = result.BasicProperties.Headers?.ToDictionary(
                pair => pair.Key,
                pair => ConvertHeaderValue(pair.Value),
                StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        };
    }

    /// <inheritdoc />
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
        return channel.ExchangeDeclareAsync(
            exchange,
            type,
            durable,
            autoDelete,
            arguments,
            passive,
            noWait,
            cancellationToken);
    }

    /// <inheritdoc />
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
        return channel.QueueDeclareAsync(
            queue,
            durable,
            exclusive,
            autoDelete,
            arguments,
            passive,
            noWait,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task QueueBindAsync(
        string queue,
        string exchange,
        string routingKey,
        IDictionary<string, object?>? arguments,
        bool noWait,
        CancellationToken cancellationToken)
    {
        return channel.QueueBindAsync(
            queue,
            exchange,
            routingKey,
            arguments,
            noWait,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task BasicPublishAsync(
        string exchange,
        string routingKey,
        bool mandatory,
        BasicProperties basicProperties,
        ReadOnlyMemory<byte> body,
        CancellationToken cancellationToken)
    {
        return channel.BasicPublishAsync(exchange, routingKey, mandatory, basicProperties, body, cancellationToken).AsTask();
    }

    /// <inheritdoc />
    public ValueTask BasicAckAsync(ulong deliveryTag, bool multiple, CancellationToken cancellationToken)
    {
        return channel.BasicAckAsync(deliveryTag, multiple, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask BasicRejectAsync(ulong deliveryTag, bool requeue, CancellationToken cancellationToken)
    {
        return channel.BasicRejectAsync(deliveryTag, requeue, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await channel.DisposeAsync().ConfigureAwait(false);
    }

    private static string ConvertHeaderValue(object? value)
    {
        return value switch
        {
            null => string.Empty,
            byte[] bytes => System.Text.Encoding.UTF8.GetString(bytes),
            ReadOnlyMemory<byte> memory => System.Text.Encoding.UTF8.GetString(memory.Span),
            _ => value.ToString() ?? string.Empty
        };
    }
}

using RabbitMQ.Client;

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
    public async ValueTask DisposeAsync()
    {
        await channel.DisposeAsync().ConfigureAwait(false);
    }
}

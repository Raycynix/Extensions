using RabbitMQ.Client;
using Raycynix.Extensions.Messaging.RabbitMQ.Internal;

namespace Raycynix.Extensions.Messaging.RabbitMQ.Interfaces;

/// <summary>
/// Minimal RabbitMQ channel abstraction used by the Raycynix RabbitMQ transport.
/// </summary>
public interface IRabbitMqChannel : IAsyncDisposable
{
    /// <summary>
    /// Configures channel prefetch behavior.
    /// </summary>
    /// <param name="prefetchSize">The maximum window size in octets.</param>
    /// <param name="prefetchCount">The maximum number of unacknowledged deliveries.</param>
    /// <param name="global">Whether QoS applies to the whole channel.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task BasicQosAsync(uint prefetchSize, ushort prefetchCount, bool global, CancellationToken cancellationToken);

    /// <summary>
    /// Declares an exchange.
    /// </summary>
    /// <param name="exchange">The exchange name.</param>
    /// <param name="type">The exchange type.</param>
    /// <param name="durable">Whether the exchange survives broker restarts.</param>
    /// <param name="autoDelete">Whether the exchange is deleted automatically.</param>
    /// <param name="arguments">Optional broker-specific arguments.</param>
    /// <param name="passive">Whether the declaration should only validate existing topology.</param>
    /// <param name="noWait">Whether the broker reply is skipped.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ExchangeDeclareAsync(
        string exchange,
        string type,
        bool durable,
        bool autoDelete,
        IDictionary<string, object?>? arguments,
        bool passive,
        bool noWait,
        CancellationToken cancellationToken);

    /// <summary>
    /// Declares a queue.
    /// </summary>
    /// <param name="queue">The queue name.</param>
    /// <param name="durable">Whether the queue survives broker restarts.</param>
    /// <param name="exclusive">Whether the queue is exclusive to the current connection.</param>
    /// <param name="autoDelete">Whether the queue is deleted automatically.</param>
    /// <param name="arguments">Optional broker-specific arguments.</param>
    /// <param name="passive">Whether the declaration should only validate existing topology.</param>
    /// <param name="noWait">Whether the broker reply is skipped.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task QueueDeclareAsync(
        string queue,
        bool durable,
        bool exclusive,
        bool autoDelete,
        IDictionary<string, object?>? arguments,
        bool passive,
        bool noWait,
        CancellationToken cancellationToken);

    /// <summary>
    /// Binds a queue to an exchange.
    /// </summary>
    /// <param name="queue">The queue name.</param>
    /// <param name="exchange">The exchange name.</param>
    /// <param name="routingKey">The routing key.</param>
    /// <param name="arguments">Optional binding arguments.</param>
    /// <param name="noWait">Whether the broker reply is skipped.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task QueueBindAsync(
        string queue,
        string exchange,
        string routingKey,
        IDictionary<string, object?>? arguments,
        bool noWait,
        CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a single message from a queue.
    /// </summary>
    /// <param name="queue">The queue name.</param>
    /// <param name="autoAck">Whether the broker should auto-acknowledge the delivery.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The retrieved delivery, or <see langword="null"/> when the queue is empty.</returns>
    Task<RabbitMqIncomingDelivery?> BasicGetAsync(
        string queue,
        bool autoAck,
        CancellationToken cancellationToken);

    /// <summary>
    /// Publishes a message.
    /// </summary>
    /// <param name="exchange">The target exchange.</param>
    /// <param name="routingKey">The routing key.</param>
    /// <param name="mandatory">Whether unroutable messages should be returned.</param>
    /// <param name="basicProperties">The AMQP basic properties.</param>
    /// <param name="body">The message payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task BasicPublishAsync(
        string exchange,
        string routingKey,
        bool mandatory,
        BasicProperties basicProperties,
        ReadOnlyMemory<byte> body,
        CancellationToken cancellationToken);

    /// <summary>
    /// Acknowledges a consumed message.
    /// </summary>
    /// <param name="deliveryTag">The delivery tag to acknowledge.</param>
    /// <param name="multiple">Whether to acknowledge multiple deliveries up to the tag.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    ValueTask BasicAckAsync(ulong deliveryTag, bool multiple, CancellationToken cancellationToken);

    /// <summary>
    /// Rejects a consumed message.
    /// </summary>
    /// <param name="deliveryTag">The delivery tag to reject.</param>
    /// <param name="requeue">Whether the delivery should be requeued.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    ValueTask BasicRejectAsync(ulong deliveryTag, bool requeue, CancellationToken cancellationToken);
}

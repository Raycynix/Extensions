using RabbitMQ.Client;

namespace Raycynix.Extensions.Messaging.RabbitMQ.Internal;

/// <summary>
/// Minimal RabbitMQ channel abstraction used by the Raycynix RabbitMQ transport.
/// </summary>
internal interface IRabbitMqChannel : IAsyncDisposable
{
    /// <summary>
    /// Configures channel prefetch behavior.
    /// </summary>
    Task BasicQosAsync(uint prefetchSize, ushort prefetchCount, bool global, CancellationToken cancellationToken);

    /// <summary>
    /// Declares an exchange.
    /// </summary>
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
    Task QueueBindAsync(
        string queue,
        string exchange,
        string routingKey,
        IDictionary<string, object?>? arguments,
        bool noWait,
        CancellationToken cancellationToken);

    /// <summary>
    /// Publishes a message.
    /// </summary>
    Task BasicPublishAsync(
        string exchange,
        string routingKey,
        bool mandatory,
        BasicProperties basicProperties,
        ReadOnlyMemory<byte> body,
        CancellationToken cancellationToken);
}

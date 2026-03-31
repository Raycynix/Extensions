namespace Raycynix.Extensions.Messaging.RabbitMQ.Internal;

/// <summary>
/// Minimal RabbitMQ connection abstraction used by the Raycynix RabbitMQ transport.
/// </summary>
internal interface IRabbitMqConnection : IAsyncDisposable
{
    /// <summary>
    /// Creates a new channel.
    /// </summary>
    Task<IRabbitMqChannel> CreateChannelAsync(CancellationToken cancellationToken);
}

namespace Raycynix.Extensions.Messaging.RabbitMQ.Interfaces;

/// <summary>
/// Minimal RabbitMQ connection abstraction used by the Raycynix RabbitMQ transport.
/// </summary>
public interface IRabbitMqConnection : IAsyncDisposable
{
    /// <summary>
    /// Creates a new channel.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A configured RabbitMQ channel abstraction.</returns>
    Task<IRabbitMqChannel> CreateChannelAsync(CancellationToken cancellationToken);
}

namespace Messaging.Abstractions
{
    /// <summary>
    /// High-level abstraction for a message bus supporting multiple transports.
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        /// Publishes a message to the specified topic or queue.
        /// </summary>
        /// <typeparam name="T">The message payload type.</typeparam>
        /// <param name="topic">The topic or queue name.</param>
        /// <param name="message">The message payload.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task PublishAsync<T>(string topic, T message, CancellationToken cancellationToken = default);
    }
}

namespace Messaging.Abstractions
{
    /// <summary>
    /// Defines contract for publishing messages to a message broker.
    /// </summary>
    public interface IMessageProducer
    {
        /// <summary>
        /// Publishes a message to a given topic or queue.
        /// </summary>
        /// <typeparam name="T">The type of the message payload.</typeparam>
        /// <param name="topic">The target topic or queue name.</param>
        /// <param name="message">The message object to publish.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task PublishAsync<T>(string topic, T message, CancellationToken cancellationToken = default);
    }
}

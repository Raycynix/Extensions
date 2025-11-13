namespace Messaging.Abstractions
{
    /// <summary>
    /// Defines contract for publishing messages to a message broker.
    /// </summary>
    public interface IMessageProducer
    {
        /// <summary>
        /// Publishes a message to the specified topic or queue.
        /// </summary>
        Task PublishAsync<T>(string topic, T message, CancellationToken cancellationToken = default);
    }
}

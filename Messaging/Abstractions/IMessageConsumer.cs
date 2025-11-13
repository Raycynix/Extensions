namespace Messaging.Abstractions
{
    /// <summary>
    /// Defines contract for consuming messages from a message broker.
    /// </summary>
    public interface IMessageConsumer
    {
        /// <summary>
        /// Subscribes to a topic or queue with a specific message type and handler.
        /// </summary>
        /// <typeparam name="TMessage">The message type.</typeparam>
        /// <typeparam name="THandler">The handler type.</typeparam>
        void Subscribe<TMessage, THandler>()
            where THandler : IMessageHandler<TMessage>;

        /// <summary>
        /// Starts consuming messages asynchronously.
        /// </summary>
        Task StartAsync(CancellationToken cancellationToken = default);
    }
}

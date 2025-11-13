namespace Messaging.Abstractions
{
    /// <summary>
    /// Defines contract for consuming messages from a message broker.
    /// </summary>
    public interface IMessageConsumer
    {
        /// <summary>
        /// Subscribes a message type to its corresponding handler.
        /// </summary>
        /// <typeparam name="TMessage">The message type.</typeparam>
        /// <typeparam name="THandler">The handler type that processes the message.</typeparam>
        void Subscribe<TMessage, THandler>()
            where THandler : IMessageHandler<TMessage>;

        /// <summary>
        /// Starts consuming messages from all configured topics or queues.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task StartAsync(CancellationToken cancellationToken = default);
    }
}

namespace Messaging.Abstractions
{
    /// <summary>
    /// Defines a contract for message handlers that process specific message types.
    /// </summary>
    /// <typeparam name="T">The message type.</typeparam>
    public interface IMessageHandler<in T>
    {
        /// <summary>
        /// Handles an incoming message asynchronously.
        /// </summary>
        /// <param name="message">The message to process.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task HandleAsync(T message, CancellationToken cancellationToken = default);
    }
}

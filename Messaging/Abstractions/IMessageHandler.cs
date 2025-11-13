namespace Messaging.Abstractions
{

    /// <summary>
    /// Represents a handler for processing incoming messages.
    /// </summary>
    /// <typeparam name="T">The message type to handle.</typeparam>
    public interface IMessageHandler<in T>
    {
        /// <summary>
        /// Handles a message of type <typeparamref name="T"/>.
        /// </summary>
        Task HandleAsync(T message, CancellationToken cancellationToken = default);
    }
}

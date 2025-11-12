namespace Messaging.Abstractions
{

    /// <summary>
    /// Represents a message handler for processing incoming messages of a specific type.
    /// </summary>
    public interface IMessageHandler<in T>
    {
        Task HandleAsync(T message, CancellationToken cancellationToken = default);
    }
}

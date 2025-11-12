namespace Messaging.Abstractions
{
    /// <summary>
    /// High-level abstraction for a message bus supporting multiple transports.
    /// </summary>
    public interface IMessageBus
    {
        Task PublishAsync<T>(string topic, T message, CancellationToken cancellationToken = default);
    }
}

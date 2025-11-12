namespace Messaging.Abstractions
{
    /// <summary>
    /// Defines contract for publishing messages to a message broker.
    /// </summary>
    public interface IMessageProducer
    {
        Task PublishAsync<T>(string topic, T message, CancellationToken cancellationToken = default);
    }
}

namespace Messaging.Abstractions
{
    /// <summary>
    /// Defines contract for consuming messages from a message broker.
    /// </summary>
    public interface IMessageConsumer
    {
        Task StartAsync(CancellationToken cancellationToken = default);
        Task StopAsync(CancellationToken cancellationToken = default);
    }
}

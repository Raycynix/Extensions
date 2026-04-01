namespace Raycynix.Extensions.Messaging.Kafka.Interfaces;

/// <summary>
/// Minimal Kafka consumer abstraction used by the Raycynix Kafka transport.
/// </summary>
public interface IKafkaConsumer : IDisposable
{
    /// <summary>
    /// Subscribes the consumer to the specified Kafka topics.
    /// </summary>
    /// <param name="topics">The topics to subscribe to.</param>
    void Subscribe(IEnumerable<string> topics);

    /// <summary>
    /// Attempts to read the next Kafka message from the subscribed topics.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The consumed message when available; otherwise, <see langword="null"/>.</returns>
    Task<KafkaIncomingMessage?> ConsumeAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Commits the consumed Kafka message offset.
    /// </summary>
    /// <param name="message">The consumed message to commit.</param>
    void Commit(KafkaIncomingMessage message);
}

using Raycynix.Extensions.Messaging.Abstractions.Models;

namespace Raycynix.Extensions.Messaging.Abstractions.Interfaces;

/// <summary>
/// Stores outgoing messages for deferred dispatch and publish recovery.
/// </summary>
public interface IMessageOutboxStore
{
    /// <summary>
    /// Enqueues a serialized message for transport delivery.
    /// </summary>
    /// <param name="message">The serialized message to enqueue.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task EnqueueAsync(SerializedMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets outbox messages available for dispatch at the specified moment.
    /// </summary>
    /// <param name="asOf">The point in time used to filter available messages.</param>
    /// <param name="maxCount">The maximum number of messages to return.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The available outbox entries.</returns>
    Task<IReadOnlyCollection<MessageOutboxEntry>> GetAvailableAsync(
        DateTimeOffset asOf,
        int maxCount,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an outbox message as dispatched successfully.
    /// </summary>
    /// <param name="messageId">The message identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task MarkDispatchedAsync(string messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an outbox message as failed and schedules its next dispatch attempt.
    /// </summary>
    /// <param name="messageId">The message identifier.</param>
    /// <param name="exception">The publish exception.</param>
    /// <param name="nextAttemptAt">The next moment when the message may be retried.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task MarkFailedAsync(
        string messageId,
        Exception exception,
        DateTimeOffset nextAttemptAt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current outbox entry for a specific message identifier.
    /// </summary>
    /// <param name="messageId">The message identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The outbox entry when found; otherwise, <see langword="null"/>.</returns>
    Task<MessageOutboxEntry?> GetAsync(string messageId, CancellationToken cancellationToken = default);
}

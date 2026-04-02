using Raycynix.Extensions.Messaging.Abstractions.Models;

namespace Raycynix.Extensions.Messaging.Abstractions.Interfaces;

/// <summary>
/// Extends the outbox store with support for deferring persistence to the ambient unit of work.
/// </summary>
public interface ITransactionalMessageOutboxStore : IMessageOutboxStore
{
    /// <summary>
    /// Gets a value indicating whether the current outbox operation should be deferred to the ambient unit of work.
    /// </summary>
    bool ShouldDeferToAmbientUnitOfWork { get; }

    /// <summary>
    /// Enqueues a serialized message without forcing an immediate persistence flush.
    /// </summary>
    /// <param name="message">The serialized message to enqueue.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task EnqueueDeferredAsync(SerializedMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Leases an outbox entry for dispatch without forcing an immediate persistence flush.
    /// </summary>
    /// <param name="messageId">The message identifier.</param>
    /// <param name="leaseUntil">The moment until which the dispatch lease is valid.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// <see langword="true"/> when the message was leased for dispatch; otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> TryBeginDispatchDeferredAsync(
        string messageId,
        DateTimeOffset leaseUntil,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an outbox message as dispatched without forcing an immediate persistence flush.
    /// </summary>
    /// <param name="messageId">The message identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task MarkDispatchedDeferredAsync(string messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an outbox message as failed without forcing an immediate persistence flush.
    /// </summary>
    /// <param name="messageId">The message identifier.</param>
    /// <param name="exception">The publish exception.</param>
    /// <param name="nextAttemptAt">The next moment when the message may be retried.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task MarkFailedDeferredAsync(
        string messageId,
        Exception exception,
        DateTimeOffset nextAttemptAt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Flushes deferred outbox changes to the underlying persistence store.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task FlushAsync(CancellationToken cancellationToken = default);
}

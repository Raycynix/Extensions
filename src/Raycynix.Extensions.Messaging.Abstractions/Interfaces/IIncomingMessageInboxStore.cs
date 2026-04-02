using Raycynix.Extensions.Messaging.Abstractions.Models;

namespace Raycynix.Extensions.Messaging.Abstractions.Interfaces;

/// <summary>
/// Stores inbox state for incoming messages to support deduplication and idempotent processing.
/// </summary>
public interface IIncomingMessageInboxStore
{
    /// <summary>
    /// Attempts to mark an incoming message as currently processing.
    /// </summary>
    /// <param name="message">The incoming transport message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// <see langword="true"/> when processing should continue; otherwise, <see langword="false"/> when the message
    /// is already being processed or has already been processed successfully.
    /// </returns>
    Task<bool> TryBeginProcessingAsync(IncomingTransportMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an incoming message as processed successfully.
    /// </summary>
    /// <param name="message">The incoming transport message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task MarkProcessedAsync(IncomingTransportMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an incoming message as failed.
    /// </summary>
    /// <param name="message">The incoming transport message.</param>
    /// <param name="exception">The processing exception.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task MarkFailedAsync(
        IncomingTransportMessage message,
        Exception exception,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets inbox state for the specified message identifier.
    /// </summary>
    /// <param name="messageId">The message identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The inbox entry when found; otherwise, <see langword="null"/>.</returns>
    Task<IncomingMessageInboxEntry?> GetAsync(string messageId, CancellationToken cancellationToken = default);
}

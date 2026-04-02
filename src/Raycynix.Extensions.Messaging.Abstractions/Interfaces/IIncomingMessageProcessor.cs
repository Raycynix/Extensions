using Raycynix.Extensions.Messaging.Abstractions.Models;

namespace Raycynix.Extensions.Messaging.Abstractions.Interfaces;

/// <summary>
/// Deserializes and dispatches incoming transport messages to registered handlers.
/// </summary>
public interface IIncomingMessageProcessor
{
    /// <summary>
    /// Processes an incoming transport message.
    /// </summary>
    /// <param name="message">The incoming transport message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ProcessAsync(IncomingTransportMessage message, CancellationToken cancellationToken = default);
}

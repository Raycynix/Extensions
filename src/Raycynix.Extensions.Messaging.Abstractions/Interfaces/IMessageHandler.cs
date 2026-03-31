using Raycynix.Extensions.Messaging.Abstractions.Models;

namespace Raycynix.Extensions.Messaging.Abstractions.Interfaces;

/// <summary>
/// Handles an incoming message envelope.
/// </summary>
/// <typeparam name="TMessage">The payload type.</typeparam>
public interface IMessageHandler<TMessage>
{
    /// <summary>
    /// Handles an incoming message.
    /// </summary>
    /// <param name="envelope">The incoming message envelope.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    ValueTask HandleAsync(MessageEnvelope<TMessage> envelope, CancellationToken cancellationToken = default);
}

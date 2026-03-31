using Raycynix.Extensions.Messaging.Abstractions.Models;

namespace Raycynix.Extensions.Messaging.Abstractions.Interfaces;

/// <summary>
/// Dispatches incoming message envelopes to registered handlers.
/// </summary>
public interface IMessageDispatcher
{
    /// <summary>
    /// Dispatches an incoming message envelope to all registered handlers for the payload type.
    /// </summary>
    /// <typeparam name="TMessage">The payload type.</typeparam>
    /// <param name="envelope">The incoming message envelope.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The dispatch result.</returns>
    ValueTask<MessageDispatchResult> DispatchAsync<TMessage>(
        MessageEnvelope<TMessage> envelope,
        CancellationToken cancellationToken = default);
}

using Raycynix.Extensions.Messaging.Abstractions.Models;

namespace Raycynix.Extensions.Messaging.Abstractions.Interfaces;

/// <summary>
/// Publishes outgoing message envelopes to the configured transport.
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publishes an outgoing message envelope.
    /// </summary>
    /// <typeparam name="TMessage">The payload type.</typeparam>
    /// <param name="envelope">The envelope to publish.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    ValueTask PublishAsync<TMessage>(MessageEnvelope<TMessage> envelope, CancellationToken cancellationToken = default);
}

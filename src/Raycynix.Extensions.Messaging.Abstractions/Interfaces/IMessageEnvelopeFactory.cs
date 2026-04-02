using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Models;

namespace Raycynix.Extensions.Messaging.Abstractions.Interfaces;

/// <summary>
/// Creates envelopes with consistent metadata for outgoing messages.
/// </summary>
public interface IMessageEnvelopeFactory
{
    /// <summary>
    /// Creates a new envelope for the supplied payload.
    /// </summary>
    /// <typeparam name="TMessage">The payload type.</typeparam>
    /// <param name="message">The payload instance.</param>
    /// <param name="destination">The logical route, topic, exchange, queue, or method name.</param>
    /// <param name="format">The payload format.</param>
    /// <param name="headers">Optional headers to include.</param>
    /// <param name="correlationId">Optional correlation identifier.</param>
    /// <param name="causationId">Optional causation identifier.</param>
    /// <returns>A populated message envelope.</returns>
    MessageEnvelope<TMessage> Create<TMessage>(
        TMessage message,
        string destination,
        MessageFormat format,
        IReadOnlyDictionary<string, string>? headers = null,
        string? correlationId = null,
        string? causationId = null);
}

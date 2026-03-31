using Raycynix.Extensions.Messaging.Abstractions.Models;

namespace Raycynix.Extensions.Messaging.Abstractions.Interfaces;

/// <summary>
/// Serializes message envelopes into transport-ready payloads.
/// </summary>
public interface IMessageSerializer
{
    /// <summary>
    /// Serializes a message envelope.
    /// </summary>
    /// <typeparam name="TMessage">The payload type.</typeparam>
    /// <param name="envelope">The envelope to serialize.</param>
    /// <returns>The serialized transport message.</returns>
    SerializedMessage Serialize<TMessage>(MessageEnvelope<TMessage> envelope);
}

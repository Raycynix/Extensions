using Raycynix.Extensions.Messaging.Abstractions.Models;

namespace Raycynix.Extensions.Messaging.Abstractions.Interfaces;

/// <summary>
/// Publishes serialized messages to a concrete transport implementation.
/// </summary>
public interface ITransportMessagePublisher
{
    /// <summary>
    /// Publishes a serialized message to the configured transport.
    /// </summary>
    /// <param name="message">The serialized message to publish.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    ValueTask PublishAsync(SerializedMessage message, CancellationToken cancellationToken = default);
}

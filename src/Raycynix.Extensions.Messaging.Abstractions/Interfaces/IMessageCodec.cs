using Raycynix.Extensions.Messaging.Abstractions.Enums;

namespace Raycynix.Extensions.Messaging.Abstractions.Interfaces;

/// <summary>
/// Serializes and deserializes messaging payloads for a specific format.
/// </summary>
public interface IMessageCodec
{
    /// <summary>
    /// Gets the logical payload format handled by the codec.
    /// </summary>
    MessageFormat Format { get; }

    /// <summary>
    /// Gets the content type produced by the codec.
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// Determines whether the codec can handle the supplied payload type.
    /// </summary>
    /// <param name="messageType">The payload type.</param>
    /// <returns><see langword="true"/> when the type is supported.</returns>
    bool CanHandle(Type messageType);

    /// <summary>
    /// Serializes a payload instance.
    /// </summary>
    /// <param name="message">The payload instance.</param>
    /// <param name="messageType">The runtime payload type.</param>
    /// <returns>The serialized payload bytes.</returns>
    byte[] Serialize(object message, Type messageType);

    /// <summary>
    /// Deserializes a payload instance.
    /// </summary>
    /// <param name="payload">The serialized payload.</param>
    /// <param name="messageType">The target payload type.</param>
    /// <returns>The deserialized payload instance.</returns>
    object Deserialize(ReadOnlyMemory<byte> payload, Type messageType);
}

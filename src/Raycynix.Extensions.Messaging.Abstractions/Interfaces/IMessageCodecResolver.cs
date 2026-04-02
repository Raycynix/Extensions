using Raycynix.Extensions.Messaging.Abstractions.Enums;

namespace Raycynix.Extensions.Messaging.Abstractions.Interfaces;

/// <summary>
/// Resolves message codecs by format and payload type.
/// </summary>
public interface IMessageCodecResolver
{
    /// <summary>
    /// Resolves a codec for the specified payload type and format.
    /// </summary>
    /// <param name="messageType">The payload type.</param>
    /// <param name="format">The payload format.</param>
    /// <returns>The matching codec.</returns>
    IMessageCodec Resolve(Type messageType, MessageFormat format);
}

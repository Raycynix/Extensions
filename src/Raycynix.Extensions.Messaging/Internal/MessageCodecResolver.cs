using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;

namespace Raycynix.Extensions.Messaging.Internal;

internal sealed class MessageCodecResolver(IEnumerable<IMessageCodec> codecs) : IMessageCodecResolver
{
    public IMessageCodec Resolve(Type messageType, MessageFormat format)
    {
        ArgumentNullException.ThrowIfNull(messageType);

        var codec = codecs.FirstOrDefault(candidate => candidate.Format == format && candidate.CanHandle(messageType));
        return codec ?? throw new InvalidOperationException(
            $"No messaging codec is registered for format '{format}' and payload type '{messageType.FullName}'.");
    }
}

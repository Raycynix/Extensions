using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;

namespace Raycynix.Extensions.Messaging.Internal;

internal sealed class MessageSerializer(IMessageCodecResolver codecResolver) : IMessageSerializer
{
    public SerializedMessage Serialize<TMessage>(MessageEnvelope<TMessage> envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var message = envelope.Message;
        var messageType = message?.GetType() ?? typeof(TMessage);
        var codec = codecResolver.Resolve(messageType, envelope.Format);
        var payload = codec.Serialize(message!, messageType);

        return new SerializedMessage
        {
            Destination = envelope.Destination,
            Payload = payload,
            Format = envelope.Format,
            ContentType = codec.ContentType,
            MessageId = envelope.MessageId,
            CorrelationId = envelope.CorrelationId,
            CausationId = envelope.CausationId,
            CreatedAt = envelope.CreatedAt,
            Headers = envelope.Headers
        };
    }
}

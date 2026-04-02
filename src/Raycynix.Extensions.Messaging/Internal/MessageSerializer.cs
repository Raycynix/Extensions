using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Abstractions.Constants;

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
        var headers = new Dictionary<string, string>(envelope.Headers, StringComparer.OrdinalIgnoreCase)
        {
            [MessageHeaderNames.Format] = envelope.Format.ToString()
        };

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
            Headers = headers
        };
    }
}

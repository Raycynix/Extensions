using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;

namespace Raycynix.Extensions.Messaging.Internal;

internal sealed class MessagePublisher(IMessageSerializer serializer) : IMessagePublisher
{
    public ValueTask PublishAsync<TMessage>(MessageEnvelope<TMessage> envelope, CancellationToken cancellationToken = default)
    {
        _ = serializer.Serialize(envelope);
        return ValueTask.CompletedTask;
    }
}

using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;

namespace Raycynix.Extensions.Messaging.Internal;

internal sealed class MessagePublisher : IMessagePublisher
{
    public ValueTask PublishAsync<TMessage>(MessageEnvelope<TMessage> envelope, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        throw new InvalidOperationException(
            "No message transport publisher is configured. Register Raycynix.Extensions.Messaging.Kafka or Raycynix.Extensions.Messaging.RabbitMQ.");
    }
}

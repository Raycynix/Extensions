using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Implementation;

namespace Raycynix.Extensions.Messaging.Internal;

internal sealed class MessagePublisher(MessageObservability observability) : IMessagePublisher
{
    /// <inheritdoc />
    public ValueTask PublishAsync<TMessage>(MessageEnvelope<TMessage> envelope, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        using var observation = observability.BeginPublish(envelope.Format.ToString().ToLowerInvariant(), envelope.Destination);

        try
        {
            throw new InvalidOperationException(
                "No message transport publisher is configured. Register Raycynix.Extensions.Messaging.Kafka or Raycynix.Extensions.Messaging.RabbitMQ.");
        }
        catch
        {
            observability.RecordPublishFailure(envelope.Format.ToString().ToLowerInvariant(), envelope.Destination);
            throw;
        }
    }
}

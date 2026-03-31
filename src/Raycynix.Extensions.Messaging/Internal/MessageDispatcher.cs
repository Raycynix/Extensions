using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;

namespace Raycynix.Extensions.Messaging.Internal;

internal sealed class MessageDispatcher(IServiceScopeFactory serviceScopeFactory) : IMessageDispatcher
{
    public async ValueTask<MessageDispatchResult> DispatchAsync<TMessage>(
        MessageEnvelope<TMessage> envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        using var scope = serviceScopeFactory.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IMessageHandler<TMessage>>().ToArray();

        foreach (var handler in handlers)
        {
            await handler.HandleAsync(envelope, cancellationToken).ConfigureAwait(false);
        }

        return new MessageDispatchResult
        {
            Context = new MessageDispatchContext
            {
                MessageType = typeof(TMessage),
                Destination = envelope.Destination,
                MessageId = envelope.MessageId,
                CorrelationId = envelope.CorrelationId,
                CausationId = envelope.CausationId,
                Contract = envelope.Contract,
                CreatedAt = envelope.CreatedAt
            },
            HandlerCount = handlers.Length
        };
    }
}

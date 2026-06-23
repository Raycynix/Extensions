using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Configurations;
using Raycynix.Extensions.Messaging.Internal;

namespace Raycynix.Extensions.Messaging.Implementations;

/// <summary>
/// Deserializes and dispatches incoming transport messages to typed handlers.
/// </summary>
internal sealed class IncomingMessageProcessor(
    IMessageCodecResolver codecResolver,
    IMessageDispatcher dispatcher,
    MessagingConfiguration configuration,
    IIncomingMessageInboxStore inboxStore,
    IncomingSecurityHeadersValidator securityHeadersValidator,
    IncomingMessageTypeResolver typeResolver,
    ILogger<IncomingMessageProcessor>? logger = null) : IIncomingMessageProcessor
{
    /// <inheritdoc />
    public async Task ProcessAsync(IncomingTransportMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        logger?.LogDebug(
            "Processing incoming message. Destination={Destination}, Format={Format}, HeaderCount={HeaderCount}, UsesInbox={UsesInbox}.",
            message.Destination,
            message.Format,
            message.Headers.Count,
            ShouldUseInbox());

        if (configuration.IncomingProcessing.ValidateSecurityHeaders)
        {
            securityHeadersValidator.Validate(message.Headers);
        }

        if (ShouldUseInbox())
        {
            var shouldProcess =
                await inboxStore.TryBeginProcessingAsync(message, cancellationToken).ConfigureAwait(false);
            if (!shouldProcess)
            {
                logger?.LogDebug(
                    "Incoming message skipped by inbox deduplication/idempotency. Destination={Destination}.",
                    message.Destination);
                return;
            }
        }

        var messageType = typeResolver.Resolve(message);
        logger?.LogDebug(
            "Resolved incoming message type. Destination={Destination}, MessageType={MessageType}.",
            message.Destination,
            messageType.FullName);
        var method = GetType()
            .GetMethod(nameof(ProcessTypedAsync),
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .MakeGenericMethod(messageType);

        try
        {
            await (Task)method.Invoke(this, [message, cancellationToken])!;

            if (ShouldUseInbox())
            {
                await inboxStore.MarkProcessedAsync(message, cancellationToken).ConfigureAwait(false);
            }

            logger?.LogDebug(
                "Incoming message processed. Destination={Destination}, MessageType={MessageType}.",
                message.Destination,
                messageType.FullName);
        }
        catch (Exception exception)
        {
            if (ShouldUseInbox())
            {
                await inboxStore.MarkFailedAsync(message, UnwrapInvocationException(exception), cancellationToken)
                    .ConfigureAwait(false);
            }

            logger?.LogWarning(
                UnwrapInvocationException(exception),
                "Incoming message processing failed. Destination={Destination}, MessageType={MessageType}.",
                message.Destination,
                messageType.FullName);

            throw;
        }
    }

    private async Task ProcessTypedAsync<TMessage>(IncomingTransportMessage message,
        CancellationToken cancellationToken)
    {
        var codec = codecResolver.Resolve(typeof(TMessage), message.Format);
        var payload = codec.Deserialize(message.Payload, typeof(TMessage));
        var envelope = new MessageEnvelope<TMessage>
        {
            Message = (TMessage)payload,
            Destination = message.Destination,
            Format = message.Format,
            MessageId = message.MessageId,
            CorrelationId = message.CorrelationId,
            CausationId = message.CausationId,
            CreatedAt = message.CreatedAt,
            Contract = typeResolver.ResolveContract(message),
            Headers = message.Headers
        };

        await dispatcher.DispatchAsync(envelope, cancellationToken).ConfigureAwait(false);
    }

    private bool ShouldUseInbox()
    {
        return configuration.IncomingProcessing.EnableDeduplication ||
               configuration.IncomingProcessing.EnableIdempotency;
    }

    private static Exception UnwrapInvocationException(Exception exception)
    {
        return exception is System.Reflection.TargetInvocationException { InnerException: not null } invocationException
            ? invocationException.InnerException!
            : exception;
    }
}
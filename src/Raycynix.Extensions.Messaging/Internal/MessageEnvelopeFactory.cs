using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Configurations;

namespace Raycynix.Extensions.Messaging.Internal;

internal sealed class MessageEnvelopeFactory(
    MessagingConfiguration configuration,
    MessageHeaderEnricher headerEnricher) : IMessageEnvelopeFactory
{
    public MessageEnvelope<TMessage> Create<TMessage>(
        TMessage message,
        string destination,
        MessageFormat format,
        IReadOnlyDictionary<string, string>? headers = null,
        string? correlationId = null,
        string? causationId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        var resolvedCorrelationId = ResolveCorrelationId(correlationId);
        var (contract, enrichedHeaders) = headerEnricher.Enrich(message, headers, resolvedCorrelationId);

        return new MessageEnvelope<TMessage>
        {
            Message = message,
            Destination = destination,
            Format = format,
            MessageId = Guid.NewGuid().ToString("N"),
            Contract = contract,
            CorrelationId = resolvedCorrelationId,
            CausationId = causationId,
            CreatedAt = DateTimeOffset.UtcNow,
            Headers = enrichedHeaders
        };
    }

    private string? ResolveCorrelationId(string? correlationId)
    {
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId;
        }

        return configuration.AutoGenerateCorrelationId ? Guid.NewGuid().ToString("N") : null;
    }
}

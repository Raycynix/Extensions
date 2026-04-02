using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.Configurations;

namespace Raycynix.Extensions.Messaging.Internal;

internal sealed class RequestEnvelopeFactory(
    MessagingConfiguration configuration,
    MessageHeaderEnricher headerEnricher) : IRequestEnvelopeFactory
{
    public RequestEnvelope<TRequest> Create<TRequest>(
        TRequest request,
        string destination,
        MessageFormat format,
        IReadOnlyDictionary<string, string>? headers = null,
        string? correlationId = null,
        TimeSpan? timeout = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        var resolvedCorrelationId = ResolveCorrelationId(correlationId);
        var (contract, enrichedHeaders) = headerEnricher.Enrich(request, headers, resolvedCorrelationId);

        return new RequestEnvelope<TRequest>
        {
            Request = request,
            Destination = destination,
            Format = format,
            RequestId = Guid.NewGuid().ToString("N"),
            Contract = contract,
            CorrelationId = resolvedCorrelationId,
            Timeout = timeout,
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

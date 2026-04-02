using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Contracts.Constants;
using Raycynix.Extensions.Contracts.Models;
using Raycynix.Extensions.Messaging.Abstractions.Constants;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Security.Abstractions.Interfaces;

namespace Raycynix.Extensions.Messaging.Internal;

internal sealed class MessageHeaderEnricher(
    IServiceProvider serviceProvider,
    IMessageContractResolver contractResolver,
    Configurations.MessagingConfiguration configuration)
{
    public (ContractMetadata Contract, IReadOnlyDictionary<string, string> Headers) Enrich<TPayload>(
        TPayload payload,
        IReadOnlyDictionary<string, string>? headers,
        string? correlationId)
    {
        var payloadType = payload?.GetType() ?? typeof(TPayload);
        var contract = contractResolver.Resolve(payloadType);
        var enrichedHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        enrichedHeaders[ContractHeaders.ContractName] = contract.Name;
        enrichedHeaders[ContractHeaders.ContractVersion] = contract.Version.ToString();

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            enrichedHeaders[MessageHeaderNames.CorrelationId] = correlationId;
        }

        if (!string.IsNullOrWhiteSpace(configuration.SourceName))
        {
            enrichedHeaders[MessageHeaderNames.Source] = configuration.SourceName;
        }

        ApplyTraceHeaders(enrichedHeaders);
        ApplySecurityHeaders(enrichedHeaders);

        if (headers is not null)
        {
            foreach (var header in headers)
            {
                if (!enrichedHeaders.ContainsKey(header.Key))
                {
                    enrichedHeaders[header.Key] = header.Value;
                }
            }
        }

        return (contract, enrichedHeaders);
    }

    private static void ApplyTraceHeaders(IDictionary<string, string> headers)
    {
        var activity = Activity.Current;
        if (activity is null)
        {
            return;
        }

        var traceParent = activity.Id ?? activity.TraceId.ToString();
        if (!string.IsNullOrWhiteSpace(traceParent))
        {
            headers[MessageHeaderNames.TraceParent] = traceParent;
        }

        if (!string.IsNullOrWhiteSpace(activity.TraceStateString))
        {
            headers[MessageHeaderNames.TraceState] = activity.TraceStateString;
        }
    }

    private void ApplySecurityHeaders(IDictionary<string, string> headers)
    {
        var securityContext = serviceProvider
            .GetServices<ISecurityContext>()
            .FirstOrDefault(static currentContext => currentContext.IsAuthenticated);
        if (securityContext is null || !securityContext.IsAuthenticated)
        {
            return;
        }

        headers[MessageHeaderNames.Authenticated] = bool.TrueString;
        headers[MessageHeaderNames.SubjectId] = securityContext.SubjectId;
        headers[MessageHeaderNames.SubjectType] = securityContext.SubjectType.ToString();

        if (securityContext.Roles.Count > 0)
        {
            headers[MessageHeaderNames.SubjectRoles] = string.Join(',', securityContext.Roles);
        }

        if (securityContext.Permissions.Count > 0)
        {
            headers[MessageHeaderNames.SubjectPermissions] = string.Join(',', securityContext.Permissions);
        }
    }
}

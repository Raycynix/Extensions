using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Raycynix.Extensions.Common.Context;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Observability.AspNetCore.Configurations;
using Raycynix.Extensions.Observability.AspNetCore.Internal;

namespace Raycynix.Extensions.Observability.AspNetCore.Http;

/// <summary>
/// Adds the current correlation identifier to outgoing HTTP requests.
/// </summary>
public class CorrelationHeaderHandler(
    IHttpContextAccessor httpContextAccessor,
    IOptions<ObservabilityAspNetCoreConfiguration>? options = null) : DelegatingHandler
{
    internal const string CorrelationHeader = "X-Correlation-ID";

    /// <summary>
    /// Sends the request and adds the correlation header when it is missing.
    /// </summary>
    /// <param name="request">The HTTP request message to send.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the request operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the HTTP response message.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!request.Headers.Contains(CorrelationHeader))
        {
            request.Headers.Add(CorrelationHeader, ResolveCorrelationId());
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private string ResolveCorrelationId()
    {
        var maximumLength = options?.Value.MaxCorrelationIdLength
                            ?? CorrelationIdNormalizer.DefaultMaximumLength;
        if (OperationContext.Current is { } operationContext)
        {
            return CorrelationIdNormalizer.NormalizeOrCreate(operationContext.CorrelationId, maximumLength);
        }

        var context = httpContextAccessor.HttpContext;
        if (context?.Request.Headers.TryGetValue(CorrelationHeader, out var headerValue) == true &&
            CorrelationIdNormalizer.TryNormalize(headerValue, maximumLength, out var incomingCorrelationId))
        {
            return incomingCorrelationId;
        }

        return Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
    }
}

using Raycynix.Extensions.Common.Context;

namespace Raycynix.Extensions.Observability.Http;

/// <summary>
/// Adds the current correlation identifier to outgoing HTTP requests.
/// </summary>
public class CorrelationHeaderHandler(IOperationContext operationContext) : DelegatingHandler
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
            request.Headers.Add(CorrelationHeader, operationContext.CorrelationId);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

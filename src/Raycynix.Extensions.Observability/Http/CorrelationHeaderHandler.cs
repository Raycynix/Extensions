using Raycynix.Extensions.Common.Context;

namespace Raycynix.Extensions.Observability.Http;

/// <summary>
/// A delegating handler for adding a correlation ID to outgoing HTTP requests.
/// This handler ensures that each HTTP request contains a correlation header, allowing
/// for tracking and correlating requests across different services.
/// </summary>
public class CorrelationHeaderHandler(IOperationContext operationContext) : DelegatingHandler
{
    private const string CorrelationHeader = "X-Correlation-ID";

    /// <summary>
    /// Sends an HTTP request with a correlation header included, ensuring the correlation information
    /// from the operation context is propagated along with the request. If the correlation header is
    /// already present, it is not overridden.
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
using Raycynix.Extensions.Common.Context;

namespace Raycynix.Extensions.Observability.Http;

public class CorrelationHeaderHandler(IOperationContext operationContext) : DelegatingHandler
{
    private const string CorrelationHeader = "X-Correlation-ID";

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
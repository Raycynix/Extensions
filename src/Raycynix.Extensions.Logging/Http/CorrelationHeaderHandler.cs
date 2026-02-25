using Raycynix.Extensions.Common.Context;

namespace Raycynix.Extensions.Logging.Http;

//TODO: Create Documentation

/// <summary>
/// 
/// </summary>
/// <param name="operationContext"></param>
public class CorrelationHeaderHandler(IOperationContext operationContext) : DelegatingHandler
{
    private const string CorrelationHeader = "X-Correlation-ID";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        // Если заголовок еще не установлен, берем его из текущего контекста операции
        if (!request.Headers.Contains(CorrelationHeader))
        {
            request.Headers.Add(CorrelationHeader, operationContext.CorrelationId);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
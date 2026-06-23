using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Models;
using Raycynix.Extensions.Messaging.HttpJson.Interfaces;

namespace Raycynix.Extensions.Messaging.HttpJson.Internal;

internal sealed class HttpJsonRequestClient(
    IHttpJsonTransport transport,
    IMessageCodecResolver codecResolver,
    ILogger<HttpJsonRequestClient>? logger = null) : IHttpJsonRequestClient
{
    public async ValueTask<ResponseEnvelope<TResponse>> SendAsync<TRequest, TResponse>(
        RequestEnvelope<TRequest> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Format != MessageFormat.Json)
        {
            throw new NotSupportedException("HTTP JSON transport supports only JSON payloads.");
        }

        var codec = codecResolver.Resolve(typeof(TRequest), request.Format);
        var payload = codec.Serialize(request.Request!, typeof(TRequest));
        logger?.LogDebug(
            "Sending HTTP JSON request. Destination={Destination}, RequestType={RequestType}, ResponseType={ResponseType}, HeaderCount={HeaderCount}, TimeoutConfigured={TimeoutConfigured}.",
            request.Destination,
            typeof(TRequest).FullName,
            typeof(TResponse).FullName,
            request.Headers.Count,
            request.Timeout is not null);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, request.Destination);
        httpRequest.Content = new ByteArrayContent(payload);

        httpRequest.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(codec.ContentType);
        httpRequest.Headers.TryAddWithoutValidation("X-Request-Id", request.RequestId);

        foreach (var header in request.Headers)
        {
            httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.CorrelationId) &&
            !httpRequest.Headers.Contains("X-Correlation-Id"))
        {
            httpRequest.Headers.TryAddWithoutValidation("X-Correlation-Id", request.CorrelationId);
        }

        var timeout = request.Timeout;
        using var linkedCancellation = timeout is { }
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            : null;

        if (linkedCancellation is not null)
        {
            linkedCancellation.CancelAfter(timeout.GetValueOrDefault());
            cancellationToken = linkedCancellation.Token;
        }

        using var response = await transport.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        var responseCodec = codecResolver.Resolve(typeof(TResponse), MessageFormat.Json);
        var model = (TResponse)responseCodec.Deserialize(bytes, typeof(TResponse));
        logger?.LogDebug(
            "Received HTTP JSON response. Destination={Destination}, StatusCode={StatusCode}, ResponseHeaderCount={ResponseHeaderCount}.",
            request.Destination,
            (int)response.StatusCode,
            response.Headers.Count() + response.Content.Headers.Count());

        return new ResponseEnvelope<TResponse>
        {
            Response = model,
            StatusCode = (int)response.StatusCode,
            CorrelationId = response.Headers.TryGetValues("X-Correlation-Id", out var values)
                ? values.FirstOrDefault()
                : request.CorrelationId,
            Headers = response.Headers
                .Concat(response.Content.Headers)
                .ToDictionary(
                    pair => pair.Key,
                    pair => string.Join(",", pair.Value),
                    StringComparer.OrdinalIgnoreCase)
        };
    }
}
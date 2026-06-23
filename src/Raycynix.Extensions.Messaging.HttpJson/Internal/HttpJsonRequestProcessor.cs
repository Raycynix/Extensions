using System.Net;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Exceptions;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.HttpJson.Interfaces;

namespace Raycynix.Extensions.Messaging.HttpJson.Internal;

/// <summary>
/// Processes inbound HTTP JSON requests through the shared request dispatcher.
/// </summary>
internal sealed class HttpJsonRequestProcessor(
    IRequestDispatcher dispatcher,
    IRequestEnvelopeFactory envelopeFactory,
    IMessageCodecResolver codecResolver,
    ILogger<HttpJsonRequestProcessor>? logger = null) : IHttpJsonRequestProcessor
{
    /// <inheritdoc />
    public async ValueTask<HttpJsonProcessedResponse> ProcessAsync<TRequest, TResponse>(
        string destination,
        ReadOnlyMemory<byte> payload,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);

        var actualHeaders = new Dictionary<string, string>(headers ?? new Dictionary<string, string>(),
            StringComparer.OrdinalIgnoreCase);
        logger?.LogDebug(
            "Processing HTTP JSON request. Destination={Destination}, RequestType={RequestType}, ResponseType={ResponseType}, HeaderCount={HeaderCount}.",
            destination,
            typeof(TRequest).FullName,
            typeof(TResponse).FullName,
            actualHeaders.Count);

        try
        {
            var requestCodec = codecResolver.Resolve(typeof(TRequest), MessageFormat.Json);
            var request = (TRequest)requestCodec.Deserialize(payload, typeof(TRequest));
            var envelope = envelopeFactory.Create(
                request,
                destination,
                MessageFormat.Json,
                headers: actualHeaders,
                correlationId: actualHeaders.TryGetValue("X-Correlation-Id", out var correlationId)
                    ? correlationId
                    : null);

            var response = await dispatcher.DispatchAsync<TRequest, TResponse>(envelope, cancellationToken)
                .ConfigureAwait(false);
            var responseCodec = codecResolver.Resolve(typeof(TResponse), MessageFormat.Json);

            return new HttpJsonProcessedResponse
            {
                StatusCode = (HttpStatusCode)response.StatusCode,
                Payload = responseCodec.Serialize(response.Response!, typeof(TResponse)),
                ContentType = responseCodec.ContentType,
                Headers = EnsureCorrelationHeader(response.Headers, response.CorrelationId)
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger?.LogWarning("HTTP JSON request processing was canceled. Destination={Destination}.", destination);
            return new HttpJsonProcessedResponse { StatusCode = HttpStatusCode.RequestTimeout };
        }
        catch (IncomingSecurityHeadersValidationException)
        {
            logger?.LogWarning("HTTP JSON request failed security header validation. Destination={Destination}.",
                destination);
            return new HttpJsonProcessedResponse { StatusCode = HttpStatusCode.Unauthorized };
        }
        catch (IncomingMessageAuthenticationException)
        {
            logger?.LogWarning("HTTP JSON request authentication failed. Destination={Destination}.", destination);
            return new HttpJsonProcessedResponse { StatusCode = HttpStatusCode.Unauthorized };
        }
        catch (IncomingMessageAuthorizationException)
        {
            logger?.LogWarning("HTTP JSON request authorization failed. Destination={Destination}.", destination);
            return new HttpJsonProcessedResponse { StatusCode = HttpStatusCode.Forbidden };
        }
        catch (InvalidOperationException exception) when (exception.Message.StartsWith(
                                                              "No request handler is registered",
                                                              StringComparison.Ordinal))
        {
            logger?.LogWarning("HTTP JSON request handler was not found. Destination={Destination}.", destination);
            return new HttpJsonProcessedResponse { StatusCode = HttpStatusCode.NotFound };
        }
        catch (Exception exception)
        {
            logger?.LogError(exception, "HTTP JSON request processing failed. Destination={Destination}.", destination);
            return new HttpJsonProcessedResponse { StatusCode = HttpStatusCode.InternalServerError };
        }
    }

    private static IReadOnlyDictionary<string, string> EnsureCorrelationHeader(
        IReadOnlyDictionary<string, string> headers,
        string? correlationId)
    {
        var actualHeaders = new Dictionary<string, string>(headers, StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            actualHeaders["X-Correlation-Id"] = correlationId;
        }

        return actualHeaders;
    }
}
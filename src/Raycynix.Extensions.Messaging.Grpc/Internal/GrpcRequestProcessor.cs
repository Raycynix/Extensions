using Grpc.Core;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Messaging.Abstractions.Enums;
using Raycynix.Extensions.Messaging.Abstractions.Exceptions;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Grpc.Interfaces;

namespace Raycynix.Extensions.Messaging.Grpc.Internal;

/// <summary>
/// Processes inbound gRPC requests through the shared request dispatcher.
/// </summary>
internal sealed class GrpcRequestProcessor(
    IRequestDispatcher dispatcher,
    IRequestEnvelopeFactory envelopeFactory,
    ILogger<GrpcRequestProcessor>? logger = null) : IGrpcRequestProcessor
{
    /// <inheritdoc />
    public async ValueTask<TResponse> ProcessAsync<TRequest, TResponse>(
        string destination,
        TRequest request,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        ArgumentNullException.ThrowIfNull(request);
        logger?.LogDebug(
            "Processing gRPC request. Destination={Destination}, RequestType={RequestType}, ResponseType={ResponseType}, HeaderCount={HeaderCount}.",
            destination,
            typeof(TRequest).FullName,
            typeof(TResponse).FullName,
            headers?.Count ?? 0);

        try
        {
            var envelope = envelopeFactory.Create(
                request,
                destination,
                MessageFormat.Grpc,
                headers: headers,
                correlationId: headers is not null && headers.TryGetValue("X-Correlation-Id", out var correlationId)
                    ? correlationId
                    : null);

            var response = await dispatcher.DispatchAsync<TRequest, TResponse>(envelope, cancellationToken)
                .ConfigureAwait(false);
            logger?.LogDebug("gRPC request processed. Destination={Destination}.", destination);
            return response.Response;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger?.LogWarning("gRPC request processing was canceled. Destination={Destination}.", destination);
            throw new RpcException(new Status(StatusCode.Cancelled, "The gRPC request was cancelled."));
        }
        catch (IncomingSecurityHeadersValidationException exception)
        {
            logger?.LogWarning(exception, "gRPC request failed security header validation. Destination={Destination}.",
                destination);
            throw new RpcException(new Status(StatusCode.Unauthenticated, exception.Message));
        }
        catch (IncomingMessageAuthenticationException exception)
        {
            logger?.LogWarning(exception, "gRPC request authentication failed. Destination={Destination}.",
                destination);
            throw new RpcException(new Status(StatusCode.Unauthenticated, exception.Message));
        }
        catch (IncomingMessageAuthorizationException exception)
        {
            logger?.LogWarning(exception, "gRPC request authorization failed. Destination={Destination}.", destination);
            throw new RpcException(new Status(StatusCode.PermissionDenied, exception.Message));
        }
        catch (InvalidOperationException exception) when (exception.Message.StartsWith(
                                                              "No request handler is registered",
                                                              StringComparison.Ordinal))
        {
            logger?.LogWarning(exception, "gRPC request handler was not found. Destination={Destination}.",
                destination);
            throw new RpcException(new Status(StatusCode.Unimplemented, exception.Message));
        }
        catch (Exception exception)
        {
            logger?.LogError(exception, "gRPC request processing failed. Destination={Destination}.", destination);
            throw new RpcException(new Status(StatusCode.Internal, exception.Message));
        }
    }
}
using Grpc.Core;
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
    IRequestEnvelopeFactory envelopeFactory) : IGrpcRequestProcessor
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

        try
        {
            var envelope = envelopeFactory.Create(
                request,
                destination,
                MessageFormat.Grpc,
                headers: headers,
                correlationId: headers is not null && headers.TryGetValue("X-Correlation-Id", out var correlationId) ? correlationId : null);

            var response = await dispatcher.DispatchAsync<TRequest, TResponse>(envelope, cancellationToken).ConfigureAwait(false);
            return response.Response;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw new RpcException(new Status(StatusCode.Cancelled, "The gRPC request was cancelled."));
        }
        catch (IncomingSecurityHeadersValidationException exception)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, exception.Message));
        }
        catch (IncomingMessageAuthenticationException exception)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, exception.Message));
        }
        catch (IncomingMessageAuthorizationException exception)
        {
            throw new RpcException(new Status(StatusCode.PermissionDenied, exception.Message));
        }
        catch (InvalidOperationException exception) when (exception.Message.StartsWith("No request handler is registered", StringComparison.Ordinal))
        {
            throw new RpcException(new Status(StatusCode.Unimplemented, exception.Message));
        }
        catch (Exception exception)
        {
            throw new RpcException(new Status(StatusCode.Internal, exception.Message));
        }
    }
}

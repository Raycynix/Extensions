namespace Raycynix.Extensions.Messaging.Grpc.Interfaces;

/// <summary>
/// Processes inbound direct gRPC requests through the shared messaging request pipeline.
/// </summary>
public interface IGrpcRequestProcessor
{
    /// <summary>
    /// Processes an inbound gRPC request for the specified destination.
    /// </summary>
    /// <typeparam name="TRequest">The request payload type.</typeparam>
    /// <typeparam name="TResponse">The response payload type.</typeparam>
    /// <param name="destination">The logical destination or method name.</param>
    /// <param name="request">The request payload.</param>
    /// <param name="headers">Optional inbound metadata headers.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response payload.</returns>
    ValueTask<TResponse> ProcessAsync<TRequest, TResponse>(
        string destination,
        TRequest request,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);
}

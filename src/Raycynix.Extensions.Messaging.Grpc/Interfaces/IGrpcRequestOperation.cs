namespace Raycynix.Extensions.Messaging.Grpc.Interfaces;

/// <summary>
/// Describes a bound gRPC request operation that can be invoked dynamically.
/// </summary>
public interface IGrpcRequestOperation
{
    /// <summary>
    /// Gets the logical destination associated with the operation.
    /// </summary>
    string Destination { get; }

    /// <summary>
    /// Gets the generated gRPC client type required to execute the operation.
    /// </summary>
    Type ClientType { get; }

    /// <summary>
    /// Gets the request payload type.
    /// </summary>
    Type RequestType { get; }

    /// <summary>
    /// Gets the response payload type.
    /// </summary>
    Type ResponseType { get; }

    /// <summary>
    /// Invokes the operation against the supplied gRPC client instance.
    /// </summary>
    /// <param name="client">The created gRPC client.</param>
    /// <param name="request">The request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The boxed response payload.</returns>
    Task<object> InvokeAsync(object client, object request, CancellationToken cancellationToken);
}

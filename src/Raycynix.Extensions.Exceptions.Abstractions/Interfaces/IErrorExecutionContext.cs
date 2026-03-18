namespace Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

/// <summary>
/// Describes the execution context in which an error occurred.
/// </summary>
public interface IErrorExecutionContext
{
    /// <summary>
    /// Gets the logical source of the failed operation, for example <c>http</c> or <c>background</c>.
    /// </summary>
    string Source { get; }

    /// <summary>
    /// Gets the logical operation name, when available.
    /// </summary>
    string? OperationName { get; }

    /// <summary>
    /// Gets the trace identifier associated with the failed operation.
    /// </summary>
    string? TraceId { get; }

    /// <summary>
    /// Gets the span identifier associated with the failed operation.
    /// </summary>
    string? SpanId { get; }

    /// <summary>
    /// Gets the correlation identifier associated with the failed operation.
    /// </summary>
    string? CorrelationId { get; }

    /// <summary>
    /// Gets the user identifier associated with the failed operation.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the request path, when the error occurred in an HTTP pipeline.
    /// </summary>
    string? Path { get; }

    /// <summary>
    /// Gets the HTTP method, when available.
    /// </summary>
    string? Method { get; }

    /// <summary>
    /// Gets the endpoint display name, when available.
    /// </summary>
    string? Endpoint { get; }

    /// <summary>
    /// Gets the HTTP query string, when available.
    /// </summary>
    string? QueryString { get; }

    /// <summary>
    /// Gets the current execution attempt number, when retry semantics are involved.
    /// </summary>
    int? Attempt { get; }

    /// <summary>
    /// Gets the maximum number of attempts that were allowed.
    /// </summary>
    int? MaxAttempts { get; }

    /// <summary>
    /// Gets a value indicating whether the failure is considered transient.
    /// </summary>
    bool IsTransient { get; }
}

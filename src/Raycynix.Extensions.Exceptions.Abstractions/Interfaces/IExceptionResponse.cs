namespace Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

/// <summary>
/// Defines the structure of an HTTP error response.
/// </summary>
public interface IExceptionResponse
{
    /// <summary>
    /// Gets the message describing the exception response.
    /// </summary>
    string Message  { get; }

    /// <summary>
    /// Gets the error code associated with the exception response.
    /// </summary>
    string ErrorCode { get; }

    /// <summary>
    /// Gets the machine-readable error category.
    /// </summary>
    string Category { get; }

    /// <summary>
    /// Gets the unique identifier representing the trace information associated with the exception response.
    /// </summary>
    string TraceId { get; }

    /// <summary>
    /// Gets the span identifier associated with the failed operation.
    /// </summary>
    string? SpanId { get; }

    /// <summary>
    /// Gets the correlation identifier associated with the failed operation.
    /// </summary>
    string? CorrelationId { get; }

    /// <summary>
    /// Gets the request path associated with the failed operation.
    /// </summary>
    string? Path { get; }

    /// <summary>
    /// Gets the HTTP method associated with the failed operation.
    /// </summary>
    string? Method { get; }

    /// <summary>
    /// Gets the route or endpoint display name associated with the failed operation.
    /// </summary>
    string? Endpoint { get; }

    /// <summary>
    /// Gets the query string associated with the failed operation.
    /// </summary>
    string? QueryString { get; }

    /// <summary>
    /// Gets the UTC timestamp when the error response was created.
    /// </summary>
    DateTimeOffset TimestampUtc { get; }

    /// <summary>
    /// Gets the machine-readable details associated with the exception.
    /// </summary>
    IReadOnlyCollection<IExceptionDetail>? Details { get; }

    /// <summary>
    /// Gets a collection of validation errors, where the key represents the field or property name,
    /// and the value contains an array of associated error messages.
    /// </summary>
    IDictionary<string, string[]>? ValidationErrors { get; }
}

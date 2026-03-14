namespace Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

/// <summary>
/// Defines a contract for representing the structure of an exception response.
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
    /// Gets the unique identifier representing the trace information associated with the exception response.
    /// </summary>
    string TraceId { get; }

    /// <summary>
    /// Gets a collection of validation errors, where the key represents the field or property name,
    /// and the value contains an array of associated error messages.
    /// </summary>
    IDictionary<string, string[]>? ValidationErrors { get; }
}
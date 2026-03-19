using Raycynix.Extensions.Exceptions.Abstractions.Enums;

namespace Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

/// <summary>
/// Describes the shared contract for Raycynix application exceptions.
/// </summary>
public interface IRaycynixException
{
    /// <summary>
    /// Gets the machine-readable error code.
    /// </summary>
    string ErrorCode { get; }

    /// <summary>
    /// Gets the status code associated with the exception.
    /// </summary>
    int StatusCode { get; }

    /// <summary>
    /// Gets the machine-readable error category.
    /// </summary>
    ErrorCategory Category { get; }

    /// <summary>
    /// Gets the human-readable error message.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Gets the public exception details associated with the error.
    /// </summary>
    IReadOnlyCollection<IExceptionDetail> Details { get; }

    /// <summary>
    /// Gets additional secure details intended for internal logging and diagnostics.
    /// </summary>
    object? SecureDetails { get; }

    /// <summary>
    /// Gets the execution context captured for the failure, when available.
    /// </summary>
    IErrorExecutionContext? ExecutionContext { get; }
}

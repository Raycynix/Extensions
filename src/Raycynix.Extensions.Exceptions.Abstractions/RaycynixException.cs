using Raycynix.Extensions.Exceptions.Abstractions.Enums;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions.Abstractions;

/// <summary>
/// Represents the base class for Raycynix application exceptions.
/// </summary>
public abstract class RaycynixException : Exception, IRaycynixException
{
    /// <summary>
    /// Gets the machine-readable error code.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Gets the HTTP status code associated with the exception.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Gets the machine-readable error category.
    /// </summary>
    public ErrorCategory Category { get; }

    /// <summary>
    /// Gets the public exception details associated with the error.
    /// </summary>
    public IReadOnlyCollection<IExceptionDetail> Details { get; }

    /// <summary>
    /// Gets optional internal details intended for logging only.
    /// </summary>
    public object? SecureDetails { get; }

    /// <summary>
    /// Gets the execution context captured for the failure, when available.
    /// </summary>
    public IErrorExecutionContext? ExecutionContext { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="RaycynixException"/>.
    /// </summary>
    protected RaycynixException(
        string message,
        string errorCode,
        int statusCode,
        ErrorCategory category,
        IReadOnlyCollection<IExceptionDetail>? details = null,
        object? secureDetails = null,
        Exception? innerException = null,
        IErrorExecutionContext? executionContext = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Category = category;
        Details = details ?? [];
        SecureDetails = secureDetails;
        ExecutionContext = executionContext;
    }

    /// <summary>
    /// Returns the details that should be used for logging.
    /// </summary>
    /// <returns>The secure details object, if available.</returns>
    public virtual object? GetLoggingDetails() => SecureDetails;
}

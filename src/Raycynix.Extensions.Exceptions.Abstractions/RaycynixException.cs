using Raycynix.Extensions.Exceptions.Abstractions.Enums;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions.Abstractions;

/// <summary>
/// Represents the base class for all custom exceptions within the Raycynix library.
/// Provides properties and mechanisms for handling error codes, status codes,
/// and optional secure details to assist in exception handling and logging.
/// </summary>
public abstract class RaycynixException : Exception, IRaycynixException
{
    /// <summary>
    /// Gets the error code associated with the exception.
    /// This value is typically used to uniquely identify the type of error
    /// and is useful for logging, diagnostics, or mapping to specific error responses.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Gets the status code associated with the exception.
    /// This value is typically used to indicate the nature of the error in responses,
    /// allowing client applications to handle responses appropriately.
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
    /// Gets an optional object containing sensitive or secure details related to the exception.
    /// This property is used to store additional contextual information that may be useful for internal logging
    /// or debugging purposes. The details contained within this property should not be exposed in public-facing responses.
    /// </summary>
    public object? SecureDetails { get; }

    /// <summary>
    /// Represents the base class for all custom exceptions within the Raycynix library.
    /// Provides properties and mechanisms for handling error codes, status codes,
    /// and optional secure details to assist in exception handling and logging.
    /// </summary>
    protected RaycynixException(
        string message,
        string errorCode,
        int statusCode,
        ErrorCategory category,
        IReadOnlyCollection<IExceptionDetail>? details = null,
        object? secureDetails = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Category = category;
        Details = details ?? [];
        SecureDetails = secureDetails;
    }

    /// <summary>
    /// Retrieves details to be used for logging purposes.
    /// By default, this method returns the secure details associated with the exception,
    /// which can be used for debugging or internal tracking.
    /// </summary>
    /// <returns>An object containing logging details or null if no secure details are available.</returns>
    public virtual object? GetLoggingDetails() => SecureDetails;
}
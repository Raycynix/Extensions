namespace Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

/// <summary>
/// Represents a base interface for defining application-specific exceptions in the Raycynix library.
/// Provides properties for managing error codes, status codes, and optional secure details.
/// </summary>
public interface IRaycynixException
{
    /// <summary>
    /// Gets the error code associated with the exception.
    /// This property is used to represent a unique identifier for the error condition,
    /// aiding in categorization, troubleshooting, and logging of issues. The error code
    /// is typically utilized in application-level exception handling to identify specific
    /// error scenarios.
    /// </summary>
    string ErrorCode { get; }

    /// <summary>
    /// Gets the status code associated with the exception.
    /// This property represents a standardized numerical indication of the specific
    /// error condition, often used in HTTP-based applications to convey response status
    /// and support client-side decision-making or logging mechanisms.
    /// </summary>
    int StatusCode { get; }

    /// <summary>
    /// Gets the message associated with the exception.
    /// This property provides a human-readable description of the exception,
    /// aiding in understanding the context or nature of the issue. It is typically
    /// utilized to convey meaningful information about the error to developers or end users.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Gets or sets additional secure details associated with the exception.
    /// This property may store sensitive or contextual information about the exception
    /// that is not intended to be exposed publicly. It is primarily used for internal
    /// logging, debugging, and diagnostic purposes.
    /// </summary>
    object? SecureDetails { get; }
}
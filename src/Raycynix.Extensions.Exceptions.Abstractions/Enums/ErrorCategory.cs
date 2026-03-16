namespace Raycynix.Extensions.Exceptions.Abstractions.Enums;

/// <summary>
/// Represents a machine-readable category of an application error.
/// </summary>
public enum ErrorCategory
{
    /// <summary>
    /// Represents an unexpected internal server error.
    /// </summary>
    Internal,

    /// <summary>
    /// Represents a validation error caused by invalid input.
    /// </summary>
    Validation,

    /// <summary>
    /// Represents a business rule violation.
    /// </summary>
    Business,

    /// <summary>
    /// Represents an authentication error.
    /// </summary>
    Unauthorized,

    /// <summary>
    /// Represents an authorization error.
    /// </summary>
    Forbidden,

    /// <summary>
    /// Represents a missing resource.
    /// </summary>
    NotFound,

    /// <summary>
    /// Represents a state conflict.
    /// </summary>
    Conflict,

    /// <summary>
    /// Represents a transient infrastructure error.
    /// </summary>
    Transient,

    /// <summary>
    /// Represents an external dependency error.
    /// </summary>
    External
}
using Raycynix.Extensions.Exceptions.Abstractions;
using Raycynix.Extensions.Exceptions.Abstractions.Enums;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions;

/// <summary>
/// Represents an exception indicating a validation failure.
/// </summary>
public class ValidationException : RaycynixException
{
    /// <summary>
    /// Gets the validation errors grouped by field name.
    /// </summary>
    public IDictionary<string, string[]> ValidationErrors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="validationErrors">The validation errors grouped by field name.</param>
    /// <param name="details">Optional machine-readable details.</param>
    /// <param name="secureDetails">Optional secure details for logging.</param>
    public ValidationException(
        string message,
        IDictionary<string, string[]> validationErrors,
        IReadOnlyCollection<IExceptionDetail>? details = null,
        object? secureDetails = null)
        : base(
            message,
            "VALIDATION_ERROR",
            400,
            ErrorCategory.Validation,
            details,
            secureDetails)
    {
        ValidationErrors = validationErrors;
    }
}
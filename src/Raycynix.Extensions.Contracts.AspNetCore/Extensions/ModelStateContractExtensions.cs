using Microsoft.AspNetCore.Mvc.ModelBinding;
using Raycynix.Extensions.Contracts.Models;

namespace Raycynix.Extensions.Contracts.AspNetCore.Extensions;

/// <summary>
/// Provides helpers for converting MVC model validation state into reusable error contracts.
/// </summary>
public static class ModelStateContractExtensions
{
    /// <summary>
    /// Converts an MVC model state dictionary into a transport-safe validation error contract.
    /// </summary>
    /// <param name="modelState">The model state dictionary.</param>
    /// <param name="code">The top-level machine-readable error code.</param>
    /// <param name="message">The top-level error message.</param>
    /// <param name="traceId">An optional trace identifier.</param>
    /// <returns>The created error contract.</returns>
    public static ErrorContract ToErrorContract(
        this ModelStateDictionary modelState,
        string code = "validation_failed",
        string message = "One or more validation errors occurred.",
        string? traceId = null)
    {
        ArgumentNullException.ThrowIfNull(modelState);

        ValidationError[] validationErrors = modelState
            .SelectMany(
                static entry => entry.Value?.Errors.Select(error => new ValidationError
                {
                    Field = entry.Key,
                    Code = "validation_error",
                    Message = string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "The supplied value is invalid."
                        : error.ErrorMessage
                }) ?? Array.Empty<ValidationError>())
            .ToArray();

        return new ErrorContract
        {
            Code = code,
            Message = message,
            TraceId = traceId,
            ValidationErrors = validationErrors
        };
    }
}

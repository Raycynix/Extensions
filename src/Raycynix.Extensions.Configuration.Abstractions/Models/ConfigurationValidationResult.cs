namespace Raycynix.Extensions.Configuration.Abstractions.Models;

/// <summary>
/// Represents the result of validating a typed configuration model.
/// </summary>
public sealed class ConfigurationValidationResult
{
    private ConfigurationValidationResult(bool succeeded, IReadOnlyCollection<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    /// <summary>
    /// Gets a value indicating whether validation succeeded.
    /// </summary>
    public bool Succeeded { get; }

    /// <summary>
    /// Gets the validation errors collected during validation.
    /// </summary>
    public IReadOnlyCollection<string> Errors { get; }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <returns>A successful <see cref="ConfigurationValidationResult"/>.</returns>
    public static ConfigurationValidationResult Success()
    {
        return new ConfigurationValidationResult(true, Array.Empty<string>());
    }

    /// <summary>
    /// Creates a failed validation result from the provided error messages.
    /// </summary>
    /// <param name="errors">The validation errors to return.</param>
    /// <returns>A failed <see cref="ConfigurationValidationResult"/>.</returns>
    public static ConfigurationValidationResult Failure(params string[] errors)
    {
        return Failure((IEnumerable<string>)errors);
    }

    /// <summary>
    /// Creates a failed validation result from the provided error messages.
    /// </summary>
    /// <param name="errors">The validation errors to return.</param>
    /// <returns>A failed <see cref="ConfigurationValidationResult"/>.</returns>
    public static ConfigurationValidationResult Failure(IEnumerable<string> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        var normalizedErrors = errors
            .Where(static error => !string.IsNullOrWhiteSpace(error))
            .ToArray();

        return new ConfigurationValidationResult(false, normalizedErrors);
    }
}

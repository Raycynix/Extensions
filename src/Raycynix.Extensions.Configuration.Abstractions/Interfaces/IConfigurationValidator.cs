using Raycynix.Extensions.Configuration.Abstractions.Models;

namespace Raycynix.Extensions.Configuration.Abstractions.Interfaces;

/// <summary>
/// Validates a typed configuration model after configuration binding has been applied.
/// </summary>
/// <typeparam name="TOptions">The configuration model type.</typeparam>
public interface IConfigurationValidator<in TOptions>
    where TOptions : class
{
    /// <summary>
    /// Validates the provided options instance.
    /// </summary>
    /// <param name="options">The bound configuration model.</param>
    /// <returns>The validation result.</returns>
    ConfigurationValidationResult Validate(TOptions options);
}

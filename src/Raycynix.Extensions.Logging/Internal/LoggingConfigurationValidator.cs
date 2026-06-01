using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;
using Raycynix.Extensions.Logging.Abstractions.Configurations;

namespace Raycynix.Extensions.Logging.Internal;

/// <summary>
/// Validates the typed Raycynix logging configuration model.
/// </summary>
internal sealed class LoggingConfigurationValidator : IConfigurationValidator<LoggingConfiguration>
{
    /// <inheritdoc />
    public ConfigurationValidationResult Validate(LoggingConfiguration options)
    {
        try
        {
            options.Validate();
            return ConfigurationValidationResult.Success();
        }
        catch (Exception exception)
        {
            return ConfigurationValidationResult.Failure(exception.Message);
        }
    }
}

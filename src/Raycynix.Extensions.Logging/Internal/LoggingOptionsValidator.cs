using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;
using Raycynix.Extensions.Logging.Abstractions.Options;

namespace Raycynix.Extensions.Logging.Internal;

/// <summary>
/// Validates the typed Raycynix logging options.
/// </summary>
internal sealed class LoggingOptionsValidator : IConfigurationValidator<LoggingOptions>
{
    /// <inheritdoc />
    public ConfigurationValidationResult Validate(LoggingOptions options)
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

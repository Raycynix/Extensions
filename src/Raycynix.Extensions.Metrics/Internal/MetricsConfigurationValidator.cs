using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;
using Raycynix.Extensions.Metrics.Configurations;

namespace Raycynix.Extensions.Metrics.Internal;

/// <summary>
/// Validates the typed Raycynix metrics configuration model.
/// </summary>
internal sealed class MetricsConfigurationValidator : IConfigurationValidator<MetricsConfiguration>
{
    /// <inheritdoc />
    public ConfigurationValidationResult Validate(MetricsConfiguration options)
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

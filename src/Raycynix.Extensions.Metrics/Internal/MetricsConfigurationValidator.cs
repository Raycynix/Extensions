using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;
using Raycynix.Extensions.Metrics.Configurations;

namespace Raycynix.Extensions.Metrics.Internal;

/// <summary>
/// Validates the typed Raycynix metrics configuration model.
/// </summary>
internal sealed class MetricsConfigurationValidator(ILogger<MetricsConfigurationValidator>? logger = null)
    : IConfigurationValidator<MetricsConfiguration>
{
    /// <inheritdoc />
    public ConfigurationValidationResult Validate(MetricsConfiguration options)
    {
        try
        {
            options.Validate();
            logger?.LogDebug(
                "Metrics configuration validated. UsePrometheus:{UsePrometheus} UseHealthChecks:{UseHealthChecks}",
                options.UsePrometheus,
                options.UseHealthChecks);

            return ConfigurationValidationResult.Success();
        }
        catch (Exception exception)
        {
            logger?.LogWarning(exception, "Metrics configuration validation failed.");
            return ConfigurationValidationResult.Failure(exception.Message);
        }
    }
}
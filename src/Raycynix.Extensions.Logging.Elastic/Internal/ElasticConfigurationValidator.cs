using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;
using Raycynix.Extensions.Logging.Elastic.Configurations;

namespace Raycynix.Extensions.Logging.Elastic.Internal;

internal sealed class ElasticConfigurationValidator : IConfigurationValidator<ElasticConfiguration>
{
    public ConfigurationValidationResult Validate(ElasticConfiguration options)
    {
        try
        {
            options.Validate();
            return ConfigurationValidationResult.Success();
        }
        catch (Exception ex)
        {
            return ConfigurationValidationResult.Failure(ex.Message);
        }
    }
}
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;
using Raycynix.Extensions.Logging.Elastic.Options;

namespace Raycynix.Extensions.Logging.Elastic.Internal;

internal sealed class ElasticOptionsValidator : IConfigurationValidator<ElasticOptions>
{
    public ConfigurationValidationResult Validate(ElasticOptions options)
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

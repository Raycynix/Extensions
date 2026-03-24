using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;
using Raycynix.Extensions.Security.Configurations;

namespace Raycynix.Extensions.Security.Internal;

internal sealed class SecurityConfigurationValidator : IConfigurationValidator<SecurityConfiguration>
{
    public ConfigurationValidationResult Validate(SecurityConfiguration options)
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

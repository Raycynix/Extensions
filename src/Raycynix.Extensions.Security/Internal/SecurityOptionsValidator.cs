using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;
using Raycynix.Extensions.Security.Options;

namespace Raycynix.Extensions.Security.Internal;

internal sealed class SecurityOptionsValidator : IConfigurationValidator<SecurityOptions>
{
    public ConfigurationValidationResult Validate(SecurityOptions options)
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

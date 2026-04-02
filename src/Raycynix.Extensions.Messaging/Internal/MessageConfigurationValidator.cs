using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;
using Raycynix.Extensions.Messaging.Configurations;

namespace Raycynix.Extensions.Messaging.Internal;

internal sealed class MessageConfigurationValidator : IConfigurationValidator<MessagingConfiguration>
{
    public ConfigurationValidationResult Validate(MessagingConfiguration options)
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

using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;
using Raycynix.Extensions.Database.Configurations;

namespace Raycynix.Extensions.Database.Internal;

internal sealed class DatabaseConfigurationValidator : IConfigurationValidator<DatabaseConfiguration>
{
    public ConfigurationValidationResult Validate(DatabaseConfiguration options)
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

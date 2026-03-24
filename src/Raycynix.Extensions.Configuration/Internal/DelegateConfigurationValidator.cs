using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;

namespace Raycynix.Extensions.Configuration.Internal;

internal sealed class DelegateConfigurationValidator<TOptions>(
    Func<TOptions, bool> validate,
    string failureMessage) : IConfigurationValidator<TOptions>
    where TOptions : class
{
    public ConfigurationValidationResult Validate(TOptions options)
    {
        return validate(options)
            ? ConfigurationValidationResult.Success()
            : ConfigurationValidationResult.Failure(failureMessage);
    }
}

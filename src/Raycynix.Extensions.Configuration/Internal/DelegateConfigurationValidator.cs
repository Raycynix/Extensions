using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;

namespace Raycynix.Extensions.Configuration.Internal;

/// <summary>
/// Adapts an inline delegate to <see cref="IConfigurationValidator{TOptions}"/>.
/// </summary>
internal sealed class DelegateConfigurationValidator<TOptions>(
    Func<TOptions, bool> validate,
    string failureMessage) : IConfigurationValidator<TOptions>
    where TOptions : class
{
    /// <inheritdoc />
    public ConfigurationValidationResult Validate(TOptions options)
    {
        return validate(options)
            ? ConfigurationValidationResult.Success()
            : ConfigurationValidationResult.Failure(failureMessage);
    }
}

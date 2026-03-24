using Microsoft.Extensions.Options;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;

namespace Raycynix.Extensions.Configuration.Internal;

internal sealed class RaycynixValidateOptions<TOptions>(
    IEnumerable<IConfigurationValidator<TOptions>> validators) : IValidateOptions<TOptions>
    where TOptions : class
{
    public ValidateOptionsResult Validate(string? name, TOptions? options)
    {
        if (options is null)
            return ValidateOptionsResult.Fail($"Configuration options of type {typeof(TOptions).Name} are missing.");

        var errors = new List<string>();

        foreach (var validator in validators)
        {
            var result = validator.Validate(options);
            if (result.Succeeded)
                continue;


            errors.AddRange(result.Errors);
        }

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }
}
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;

namespace Raycynix.Extensions.Configuration.Internal;

/// <summary>
/// Aggregates Raycynix configuration validators into the standard options validation pipeline.
/// </summary>
internal sealed class RaycynixValidateOptions<TOptions>(
    IEnumerable<IConfigurationValidator<TOptions>> validators,
    ILogger<RaycynixValidateOptions<TOptions>>? logger = null) : IValidateOptions<TOptions>
    where TOptions : class
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, TOptions? options)
    {
        var optionsName = string.IsNullOrWhiteSpace(name) ? Options.DefaultName : name;

        if (options is null)
        {
            logger?.LogWarning(
                "Configuration validation failed for options type {OptionsType} with name {OptionsName} because options are missing.",
                typeof(TOptions).Name,
                optionsName);

            return ValidateOptionsResult.Fail($"Configuration options of type {typeof(TOptions).Name} are missing.");
        }

        var validatorList = validators.ToArray();
        logger?.LogDebug(
            "Running {ValidatorCount} configuration validators for options type {OptionsType} with name {OptionsName}.",
            validatorList.Length,
            typeof(TOptions).Name,
            optionsName);

        var errors = new List<string>();

        foreach (var validator in validatorList)
        {
            var result = validator.Validate(options);
            if (result.Succeeded)
            {
                logger?.LogDebug(
                    "Configuration validator {ValidatorType} succeeded for options type {OptionsType} with name {OptionsName}.",
                    validator.GetType().Name,
                    typeof(TOptions).Name,
                    optionsName);

                continue;
            }

            logger?.LogWarning(
                "Configuration validator {ValidatorType} failed for options type {OptionsType} with name {OptionsName}. Error count: {ErrorCount}.",
                validator.GetType().Name,
                typeof(TOptions).Name,
                optionsName,
                result.Errors.Count);

            errors.AddRange(result.Errors);
        }

        logger?.LogDebug(
            "Configuration validation completed for options type {OptionsType} with name {OptionsName}. Succeeded: {Succeeded}.",
            typeof(TOptions).Name,
            optionsName,
            errors.Count == 0);

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }
}

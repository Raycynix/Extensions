using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;
using Raycynix.Extensions.Secrets.Options;

namespace Raycynix.Extensions.Secrets.Internal;

internal sealed class SecretOptionsValidator : IConfigurationValidator<SecretOptions>
{
    public ConfigurationValidationResult Validate(SecretOptions options)
    {
        if (options.ProviderOrder is null)
        {
            return ConfigurationValidationResult.Failure(
                "SecretOptions.ProviderOrder must be provided.");
        }

        var errors = new List<string>();
        var providerNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var providerName in options.ProviderOrder)
        {
            if (string.IsNullOrWhiteSpace(providerName))
            {
                errors.Add("SecretOptions.ProviderOrder cannot contain an empty provider name.");
                continue;
            }

            if (!providerNames.Add(providerName.Trim()))
            {
                errors.Add(
                    $"SecretOptions.ProviderOrder contains duplicate provider name '{providerName.Trim()}'.");
            }
        }

        return errors.Count == 0
            ? ConfigurationValidationResult.Success()
            : ConfigurationValidationResult.Failure(errors);
    }
}

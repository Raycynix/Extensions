using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;

namespace Raycynix.Extensions.Configuration.Example;

internal sealed class MessagingOptionsValidator : IConfigurationValidator<MessagingOptions>
{
    public ConfigurationValidationResult Validate(MessagingOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            errors.Add("MessagingOptions.ConnectionString is required.");
        }

        if (string.IsNullOrWhiteSpace(options.ConsumerName))
        {
            errors.Add("MessagingOptions.ConsumerName is required.");
        }

        if (options.BatchSize <= 0)
        {
            errors.Add("MessagingOptions.BatchSize must be greater than zero.");
        }

        if (options.PrefetchCount < options.BatchSize)
        {
            errors.Add("MessagingOptions.PrefetchCount must be greater than or equal to BatchSize.");
        }

        return errors.Count == 0
            ? ConfigurationValidationResult.Success()
            : ConfigurationValidationResult.Failure(errors);
    }
}
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;
using Raycynix.Extensions.Email.Configurations;
using Raycynix.Extensions.Email.Abstractions.Models;

namespace Raycynix.Extensions.Email.Internal;

internal sealed class EmailConfigurationValidator : IConfigurationValidator<EmailConfiguration>
{
    public ConfigurationValidationResult Validate(EmailConfiguration options)
    {
        var errors = new List<string>();

        ValidateEmailAddress(options.DefaultFromAddress, nameof(options.DefaultFromAddress), errors);
        ValidateEmailAddress(options.DefaultReplyToAddress, nameof(options.DefaultReplyToAddress), errors);
        ValidateDisplayName(options.DefaultFromDisplayName, nameof(options.DefaultFromDisplayName), errors);
        ValidateDisplayName(options.DefaultReplyToDisplayName, nameof(options.DefaultReplyToDisplayName), errors);

        return errors.Count == 0
            ? ConfigurationValidationResult.Success()
            : ConfigurationValidationResult.Failure(errors);
    }

    private static void ValidateEmailAddress(string? address, string name, ICollection<string> errors)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return;
        }

        try
        {
            _ = new EmailAddress(address);
        }
        catch (ArgumentException)
        {
            errors.Add($"{name} must be a valid email address.");
        }
    }

    private static void ValidateDisplayName(string? displayName, string name, ICollection<string> errors)
    {
        if (displayName?.Length > 255)
        {
            errors.Add($"{name} cannot exceed 255 characters.");
        }
    }
}

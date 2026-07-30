using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;
using Raycynix.Extensions.Email.Smtp.Options;

namespace Raycynix.Extensions.Email.Smtp.Internal;

internal sealed class SmtpOptionsValidator : IConfigurationValidator<SmtpOptions>
{
    public ConfigurationValidationResult Validate(SmtpOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Host))
        {
            errors.Add("SMTP host is required.");
        }

        if (options.Port is <= 0 or > 65535)
        {
            errors.Add("SMTP port must be between 1 and 65535.");
        }

        if (!Enum.IsDefined(options.SecureSocketOptions))
        {
            errors.Add("SMTP secure socket options value is not supported.");
        }

        if (options.TimeoutMilliseconds <= 0)
        {
            errors.Add("SMTP timeout must be greater than zero.");
        }

        if (options.UseDefaultCredentials && (!string.IsNullOrWhiteSpace(options.Username) ||
                                             !string.IsNullOrWhiteSpace(options.Password)))
        {
            errors.Add("SMTP default credentials cannot be combined with explicit username or password.");
        }

        if (!options.UseDefaultCredentials &&
            string.IsNullOrWhiteSpace(options.Username) != string.IsNullOrWhiteSpace(options.Password))
        {
            errors.Add("SMTP username and password must be configured together.");
        }

        return errors.Count == 0
            ? ConfigurationValidationResult.Success()
            : ConfigurationValidationResult.Failure(errors);
    }
}

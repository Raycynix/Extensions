using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Email.Abstractions.Exceptions;
using Raycynix.Extensions.Email.Abstractions.Interfaces;
using Raycynix.Extensions.Email.Smtp.Options;

namespace Raycynix.Extensions.Email.Smtp.Internal;

internal sealed class SmtpEmailProviderRegistration(
    ILogger<SmtpEmailProviderRegistration>? logger = null) : IEmailProviderRegistration
{
    public string ProviderName => "smtp";

    public void Validate(IServiceProvider serviceProvider)
    {
        logger?.LogDebug("Validating SMTP email provider options.");

        var options = serviceProvider.GetService(typeof(SmtpOptions)) as SmtpOptions
                      ?? throw new EmailProviderConfigurationException(
                          "SMTP configuration is not registered.");

        var result = new SmtpOptionsValidator().Validate(options);
        if (!result.Succeeded)
        {
            logger?.LogWarning(
                "SMTP email provider configuration validation failed. ErrorCount={ErrorCount}.",
                result.Errors.Count);

            throw new EmailProviderConfigurationException(string.Join(" ", result.Errors));
        }

        logger?.LogDebug(
            "SMTP email provider configuration validation completed. HostConfigured={HostConfigured}, Port={Port}, SecureSocketOptions={SecureSocketOptions}, UsesDefaultCredentials={UsesDefaultCredentials}, ExplicitCredentialsConfigured={ExplicitCredentialsConfigured}.",
            !string.IsNullOrWhiteSpace(options.Host),
            options.Port,
            options.SecureSocketOptions,
            options.UseDefaultCredentials,
            !string.IsNullOrWhiteSpace(options.Username));
    }
}

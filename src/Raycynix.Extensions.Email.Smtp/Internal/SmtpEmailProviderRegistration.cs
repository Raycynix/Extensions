using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Email.Abstractions.Exceptions;
using Raycynix.Extensions.Email.Abstractions.Interfaces;
using Raycynix.Extensions.Email.Smtp.Configurations;

namespace Raycynix.Extensions.Email.Smtp.Internal;

internal sealed class SmtpEmailProviderRegistration(
    ILogger<SmtpEmailProviderRegistration>? logger = null) : IEmailProviderRegistration
{
    public string ProviderName => "smtp";

    public void Validate(IServiceProvider serviceProvider)
    {
        logger?.LogDebug("Validating SMTP email provider configuration.");

        var configuration = serviceProvider.GetService(typeof(SmtpConfiguration)) as SmtpConfiguration
                            ?? throw new EmailProviderConfigurationException(
                                "SMTP configuration is not registered.");

        var result = new SmtpConfigurationValidator().Validate(configuration);
        if (!result.Succeeded)
        {
            logger?.LogWarning(
                "SMTP email provider configuration validation failed. ErrorCount={ErrorCount}.",
                result.Errors.Count);

            throw new EmailProviderConfigurationException(string.Join(" ", result.Errors));
        }

        logger?.LogDebug(
            "SMTP email provider configuration validation completed. HostConfigured={HostConfigured}, Port={Port}, SecureSocketOptions={SecureSocketOptions}, UsesDefaultCredentials={UsesDefaultCredentials}, ExplicitCredentialsConfigured={ExplicitCredentialsConfigured}.",
            !string.IsNullOrWhiteSpace(configuration.Host),
            configuration.Port,
            configuration.SecureSocketOptions,
            configuration.UseDefaultCredentials,
            !string.IsNullOrWhiteSpace(configuration.Username));
    }
}

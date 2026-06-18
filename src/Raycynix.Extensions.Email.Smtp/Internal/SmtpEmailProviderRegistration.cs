using Raycynix.Extensions.Email.Abstractions.Exceptions;
using Raycynix.Extensions.Email.Abstractions.Interfaces;
using Raycynix.Extensions.Email.Smtp.Configurations;

namespace Raycynix.Extensions.Email.Smtp.Internal;

internal sealed class SmtpEmailProviderRegistration : IEmailProviderRegistration
{
    public string ProviderName => "smtp";

    public void Validate(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetService(typeof(SmtpConfiguration)) as SmtpConfiguration
                            ?? throw new EmailProviderConfigurationException(
                                "SMTP configuration is not registered.");

        if (string.IsNullOrWhiteSpace(configuration.Host))
        {
            throw new EmailProviderConfigurationException("SMTP host is required.");
        }

        if (configuration.Port is <= 0 or > 65535)
        {
            throw new EmailProviderConfigurationException("SMTP port must be between 1 and 65535.");
        }

    }
}

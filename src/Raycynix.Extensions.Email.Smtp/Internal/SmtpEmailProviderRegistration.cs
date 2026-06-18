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

        var result = new SmtpConfigurationValidator().Validate(configuration);
        if (!result.Succeeded)
        {
            throw new EmailProviderConfigurationException(string.Join(" ", result.Errors));
        }
    }
}

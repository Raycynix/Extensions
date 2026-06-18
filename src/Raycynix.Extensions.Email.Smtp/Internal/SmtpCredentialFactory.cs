using System.Net;
using Raycynix.Extensions.Email.Smtp.Configurations;

namespace Raycynix.Extensions.Email.Smtp.Internal;

internal static class SmtpCredentialFactory
{
    public static ICredentials? Create(SmtpConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (configuration.UseDefaultCredentials)
        {
            return CredentialCache.DefaultNetworkCredentials;
        }

        return string.IsNullOrWhiteSpace(configuration.Username)
            ? null
            : new NetworkCredential(configuration.Username, configuration.Password ?? string.Empty);
    }
}

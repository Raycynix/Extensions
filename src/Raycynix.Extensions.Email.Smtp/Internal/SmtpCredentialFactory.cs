using System.Net;
using Raycynix.Extensions.Email.Smtp.Options;

namespace Raycynix.Extensions.Email.Smtp.Internal;

internal static class SmtpCredentialFactory
{
    public static ICredentials? Create(SmtpOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.UseDefaultCredentials)
        {
            return CredentialCache.DefaultNetworkCredentials;
        }

        return string.IsNullOrWhiteSpace(options.Username)
            ? null
            : new NetworkCredential(options.Username, options.Password ?? string.Empty);
    }
}

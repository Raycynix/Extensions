using MailKit.Security;
using Raycynix.Extensions.Email.Smtp.Options;
using Raycynix.Extensions.Email.Smtp.Enums;

namespace Raycynix.Extensions.Email.Smtp.Internal;

internal static class SmtpSecureSocketOptionsMapper
{
    public static SecureSocketOptions Map(SmtpOptions configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return configuration.SecureSocketOptions switch
        {
            SmtpSecureSocketOptions.None => SecureSocketOptions.None,
            SmtpSecureSocketOptions.StartTls => SecureSocketOptions.StartTls,
            SmtpSecureSocketOptions.StartTlsWhenAvailable => SecureSocketOptions.StartTlsWhenAvailable,
            SmtpSecureSocketOptions.SslOnConnect => SecureSocketOptions.SslOnConnect,
            _ => SecureSocketOptions.Auto
        };
    }
}

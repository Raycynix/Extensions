namespace Raycynix.Extensions.Email.Smtp.Enums;

/// <summary>
/// Defines the SMTP transport security mode.
/// </summary>
public enum SmtpSecureSocketOptions
{
    /// <summary>
    /// Automatically selects a transport security mode from the configured port and SSL setting.
    /// </summary>
    Auto,

    /// <summary>
    /// Uses an unencrypted SMTP connection.
    /// </summary>
    None,

    /// <summary>
    /// Uses STARTTLS and fails when TLS is unavailable.
    /// </summary>
    StartTls,

    /// <summary>
    /// Uses STARTTLS when the server supports it.
    /// </summary>
    StartTlsWhenAvailable,

    /// <summary>
    /// Uses an SSL/TLS-wrapped connection from the start.
    /// </summary>
    SslOnConnect
}

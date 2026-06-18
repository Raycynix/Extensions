using Raycynix.Extensions.Email.Smtp.Enums;

namespace Raycynix.Extensions.Email.Smtp.Configurations;

/// <summary>
/// Represents SMTP provider settings.
/// </summary>
public sealed class SmtpConfiguration
{
    /// <summary>
    /// Gets or sets the SMTP server host name.
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// Gets or sets the SMTP server port.
    /// </summary>
    public int Port { get; set; } = 25;

    /// <summary>
    /// Gets or sets a value indicating whether SSL should be enabled.
    /// </summary>
    public bool EnableSsl { get; set; }

    /// <summary>
    /// Gets or sets the SMTP transport security mode.
    /// </summary>
    public SmtpSecureSocketOptions SecureSocketOptions { get; set; } = SmtpSecureSocketOptions.Auto;

    /// <summary>
    /// Gets or sets the SMTP username.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the SMTP password.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether default credentials should be used.
    /// </summary>
    public bool UseDefaultCredentials { get; set; }

    /// <summary>
    /// Gets or sets the send timeout in milliseconds.
    /// </summary>
    public int TimeoutMilliseconds { get; set; } = 100000;
}

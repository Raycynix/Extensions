using FluentAssertions;
using MailKit.Security;
using Raycynix.Extensions.Email.Smtp.Configurations;
using Raycynix.Extensions.Email.Smtp.Enums;
using Raycynix.Extensions.Email.Smtp.Internal;

namespace Raycynix.Extensions.Email.Tests.Smtp;

/// <summary>
/// Covers SMTP secure socket option mapping.
/// </summary>
public sealed class SmtpSecureSocketOptionsMapperTests
{
    /// <summary>
    /// Verifies that default Auto is preserved for MailKit instead of falling back to plaintext.
    /// </summary>
    [Fact]
    public void Map_ShouldPreserveAuto()
    {
        var configuration = new SmtpConfiguration
        {
            Port = 587
        };

        var options = SmtpSecureSocketOptionsMapper.Map(configuration);

        options.Should().Be(SecureSocketOptions.Auto);
    }

    /// <summary>
    /// Verifies explicit secure socket options are mapped directly.
    /// </summary>
    [Theory]
    [InlineData(SmtpSecureSocketOptions.None, SecureSocketOptions.None)]
    [InlineData(SmtpSecureSocketOptions.StartTls, SecureSocketOptions.StartTls)]
    [InlineData(SmtpSecureSocketOptions.StartTlsWhenAvailable, SecureSocketOptions.StartTlsWhenAvailable)]
    [InlineData(SmtpSecureSocketOptions.SslOnConnect, SecureSocketOptions.SslOnConnect)]
    public void Map_ShouldPreserveExplicitOptions(
        SmtpSecureSocketOptions configured,
        SecureSocketOptions expected)
    {
        var configuration = new SmtpConfiguration
        {
            SecureSocketOptions = configured
        };

        var options = SmtpSecureSocketOptionsMapper.Map(configuration);

        options.Should().Be(expected);
    }
}

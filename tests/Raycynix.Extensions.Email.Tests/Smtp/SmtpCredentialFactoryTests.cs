using System.Net;
using FluentAssertions;
using Raycynix.Extensions.Email.Smtp.Configurations;
using Raycynix.Extensions.Email.Smtp.Internal;

namespace Raycynix.Extensions.Email.Tests.Smtp;

/// <summary>
/// Covers SMTP credential selection.
/// </summary>
public sealed class SmtpCredentialFactoryTests
{
    /// <summary>
    /// Verifies that default credentials are returned when requested.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnDefaultNetworkCredentials_WhenDefaultCredentialsAreRequested()
    {
        var configuration = new SmtpConfiguration
        {
            UseDefaultCredentials = true
        };

        var credentials = SmtpCredentialFactory.Create(configuration);

        credentials.Should().BeSameAs(CredentialCache.DefaultNetworkCredentials);
    }

    /// <summary>
    /// Verifies that explicit username and password are converted to network credentials.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnNetworkCredential_WhenUsernameIsConfigured()
    {
        var configuration = new SmtpConfiguration
        {
            Username = "smtp-user",
            Password = "smtp-password"
        };

        var credentials = SmtpCredentialFactory.Create(configuration);

        credentials.Should().BeOfType<NetworkCredential>();
        var networkCredential = (NetworkCredential)credentials;
        networkCredential.UserName.Should().Be("smtp-user");
        networkCredential.Password.Should().Be("smtp-password");
    }

    /// <summary>
    /// Verifies that no credentials are returned when authentication is not configured.
    /// </summary>
    [Fact]
    public void Create_ShouldReturnNull_WhenAuthenticationIsNotConfigured()
    {
        var configuration = new SmtpConfiguration();

        var credentials = SmtpCredentialFactory.Create(configuration);

        credentials.Should().BeNull();
    }
}

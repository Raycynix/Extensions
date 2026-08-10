using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Email.Abstractions.Exceptions;
using Raycynix.Extensions.Email.Abstractions.Interfaces;
using Raycynix.Extensions.Email.Abstractions.Models;
using Raycynix.Extensions.Email.Implementations;
using Raycynix.Extensions.Email.Smtp;
using Raycynix.Extensions.Email.Smtp.Options;
using Raycynix.Extensions.Email.Smtp.Enums;

namespace Raycynix.Extensions.Email.Tests.Registration;

/// <summary>
/// Covers email service and provider registration behavior.
/// </summary>
public sealed class EmailRegistrationTests
{
    /// <summary>
    /// Verifies that SMTP configuration is bound from the nested options sections.
    /// </summary>
    [Fact]
    public void AddSmtp_ShouldBindSmtpOptions_FromNestedOptionsSections()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["EmailOptions:DefaultFromAddress"] = "no-reply@example.com",
            ["EmailOptions:SmtpOptions:Host"] = "smtp.example.com",
            ["EmailOptions:SmtpOptions:Port"] = "587",
            ["EmailOptions:SmtpOptions:SecureSocketOptions"] = "StartTls",
            ["EmailOptions:SmtpOptions:Username"] = "smtp-user",
            ["EmailOptions:SmtpOptions:Password"] = "smtp-password"
        });

        services
            .AddRaycynixEmail(configuration)
            .AddSmtp();

        using var provider = services.BuildServiceProvider();
        var smtp = provider.GetRequiredService<SmtpOptions>();

        smtp.Host.Should().Be("smtp.example.com");
        smtp.Port.Should().Be(587);
        smtp.SecureSocketOptions.Should().Be(SmtpSecureSocketOptions.StartTls);
        smtp.Username.Should().Be("smtp-user");
        smtp.Password.Should().Be("smtp-password");
    }

    /// <summary>
    /// Verifies that SMTP registration exposes the email sender contract.
    /// </summary>
    [Fact]
    public void AddSmtp_ShouldRegisterEmailSender()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["EmailOptions:DefaultFromAddress"] = "no-reply@example.com",
            ["EmailOptions:SmtpOptions:Host"] = "smtp.example.com"
        });

        services
            .AddRaycynixEmail(configuration)
            .AddSmtp();

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IEmailSender>().Should().NotBeNull();
        provider.GetRequiredService<EmailProviderDescriptor>().ProviderName.Should().Be("smtp");
    }

    /// <summary>
    /// Verifies that provider resolution fails when no provider package is registered.
    /// </summary>
    [Fact]
    public void EmailProviderDescriptor_ShouldThrow_WhenNoProviderIsRegistered()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();
        services.AddRaycynixEmail(configuration);

        using var provider = services.BuildServiceProvider();
        var act = () => provider.GetRequiredService<EmailProviderDescriptor>();

        act.Should().Throw<NotSupportedException>();
    }

    /// <summary>
    /// Verifies that provider resolution fails when multiple email providers are registered.
    /// </summary>
    [Fact]
    public void EmailProviderDescriptor_ShouldThrow_WhenMultipleProvidersAreRegistered()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["EmailOptions:SmtpOptions:Host"] = "smtp.example.com"
        });
        services
            .AddRaycynixEmail(configuration)
            .AddSmtp();
        services.AddSingleton<IEmailProviderRegistration, TestEmailProviderRegistration>();

        using var provider = services.BuildServiceProvider();
        var act = () => provider.GetRequiredService<EmailProviderDescriptor>();

        act.Should().Throw<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that SMTP sender requires a default or per-message sender before connecting to SMTP.
    /// </summary>
    [Fact]
    public async Task SmtpSender_ShouldThrow_WhenSenderAddressIsMissing()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["EmailOptions:SmtpOptions:Host"] = "smtp.example.com"
        });
        services
            .AddRaycynixEmail(configuration)
            .AddSmtp();

        using var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<IEmailSender>();
        var message = new EmailMessage
        {
            To = [new EmailAddress("user@example.com")],
            Subject = "Hello",
            Body = EmailBody.FromPlainText("Hello")
        };

        var act = () => sender.SendAsync(message);

        await act.Should().ThrowAsync<EmailSendException>()
            .WithMessage("Email message requires a sender address.*");
    }

    /// <summary>
    /// Verifies that runtime SMTP provider validation uses the same timeout rules as configuration validation.
    /// </summary>
    [Fact]
    public void SmtpSender_ShouldThrow_WhenTimeoutIsInvalid()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["EmailOptions:DefaultFromAddress"] = "no-reply@example.com",
            ["EmailOptions:SmtpOptions:Host"] = "smtp.example.com",
            ["EmailOptions:SmtpOptions:TimeoutMilliseconds"] = "0"
        });
        services
            .AddRaycynixEmail(configuration)
            .AddSmtp();

        using var provider = services.BuildServiceProvider();
        var act = () => provider.GetRequiredService<IEmailSender>();

        act.Should().Throw<OptionsValidationException>()
            .WithMessage("SMTP timeout must be greater than zero.");
    }

    /// <summary>
    /// Verifies that runtime SMTP provider validation rejects conflicting authentication modes.
    /// </summary>
    [Fact]
    public void SmtpSender_ShouldThrow_WhenDefaultCredentialsAreCombinedWithExplicitCredentials()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["EmailOptions:DefaultFromAddress"] = "no-reply@example.com",
            ["EmailOptions:SmtpOptions:Host"] = "smtp.example.com",
            ["EmailOptions:SmtpOptions:UseDefaultCredentials"] = "true",
            ["EmailOptions:SmtpOptions:Username"] = "smtp-user"
        });
        services
            .AddRaycynixEmail(configuration)
            .AddSmtp();

        using var provider = services.BuildServiceProvider();
        var act = () => provider.GetRequiredService<IEmailSender>();

        act.Should().Throw<OptionsValidationException>()
            .WithMessage("SMTP default credentials cannot be combined with explicit username or password.");
    }

    /// <summary>
    /// Verifies that explicit SMTP credentials are configured as a complete pair.
    /// </summary>
    [Fact]
    public void SmtpSender_ShouldThrow_WhenOnlyUsernameIsConfigured()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["EmailOptions:DefaultFromAddress"] = "no-reply@example.com",
            ["EmailOptions:SmtpOptions:Host"] = "smtp.example.com",
            ["EmailOptions:SmtpOptions:Username"] = "smtp-user"
        });
        services
            .AddRaycynixEmail(configuration)
            .AddSmtp();

        using var provider = services.BuildServiceProvider();
        var act = () => provider.GetRequiredService<IEmailSender>();

        act.Should().Throw<OptionsValidationException>()
            .WithMessage("SMTP username and password must be configured together.");
    }

    private static IConfiguration CreateConfiguration(
        Dictionary<string, string?>? values = null)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values ?? [])
            .Build();
    }

    private sealed class TestEmailProviderRegistration : IEmailProviderRegistration
    {
        public string ProviderName => "test";

        public void Validate(IServiceProvider serviceProvider)
        {
        }
    }
}

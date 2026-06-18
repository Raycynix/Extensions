using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Email.Abstractions.Exceptions;
using Raycynix.Extensions.Email.Abstractions.Interfaces;
using Raycynix.Extensions.Email.Abstractions.Models;
using Raycynix.Extensions.Email.Implementations;
using Raycynix.Extensions.Email.Smtp;
using Raycynix.Extensions.Email.Smtp.Configurations;
using Raycynix.Extensions.Email.Smtp.Enums;

namespace Raycynix.Extensions.Email.Tests.Registration;

/// <summary>
/// Covers email service and provider registration behavior.
/// </summary>
public sealed class EmailRegistrationTests
{
    /// <summary>
    /// Verifies that SMTP configuration is bound from the nested EmailConfiguration section.
    /// </summary>
    [Fact]
    public void AddSmtp_ShouldBindSmtpConfiguration_FromNestedEmailConfigurationSection()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["EmailConfiguration:DefaultFromAddress"] = "no-reply@example.com",
            ["EmailConfiguration:SmtpConfiguration:Host"] = "smtp.example.com",
            ["EmailConfiguration:SmtpConfiguration:Port"] = "587",
            ["EmailConfiguration:SmtpConfiguration:SecureSocketOptions"] = "StartTls",
            ["EmailConfiguration:SmtpConfiguration:Username"] = "smtp-user",
            ["EmailConfiguration:SmtpConfiguration:Password"] = "smtp-password"
        });

        services
            .AddRaycynixEmail(configuration)
            .AddSmtp();

        using var provider = services.BuildServiceProvider();
        var smtp = provider.GetRequiredService<SmtpConfiguration>();

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
            ["EmailConfiguration:DefaultFromAddress"] = "no-reply@example.com",
            ["EmailConfiguration:SmtpConfiguration:Host"] = "smtp.example.com"
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
            ["EmailConfiguration:SmtpConfiguration:Host"] = "smtp.example.com"
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
            ["EmailConfiguration:SmtpConfiguration:Host"] = "smtp.example.com"
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
            ["EmailConfiguration:DefaultFromAddress"] = "no-reply@example.com",
            ["EmailConfiguration:SmtpConfiguration:Host"] = "smtp.example.com",
            ["EmailConfiguration:SmtpConfiguration:TimeoutMilliseconds"] = "-1"
        });
        services
            .AddRaycynixEmail(configuration)
            .AddSmtp();

        using var provider = services.BuildServiceProvider();
        var act = () => provider.GetRequiredService<IEmailSender>();

        act.Should().Throw<OptionsValidationException>()
            .WithMessage("SMTP timeout cannot be negative.");
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
            ["EmailConfiguration:DefaultFromAddress"] = "no-reply@example.com",
            ["EmailConfiguration:SmtpConfiguration:Host"] = "smtp.example.com",
            ["EmailConfiguration:SmtpConfiguration:UseDefaultCredentials"] = "true",
            ["EmailConfiguration:SmtpConfiguration:Username"] = "smtp-user"
        });
        services
            .AddRaycynixEmail(configuration)
            .AddSmtp();

        using var provider = services.BuildServiceProvider();
        var act = () => provider.GetRequiredService<IEmailSender>();

        act.Should().Throw<OptionsValidationException>()
            .WithMessage("SMTP default credentials cannot be combined with explicit username or password.");
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

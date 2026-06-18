using System.Net.Sockets;
using MailKit.Net.Smtp;
using MailKit.Security;
using Raycynix.Extensions.Email.Abstractions.Exceptions;
using Raycynix.Extensions.Email.Abstractions.Interfaces;
using Raycynix.Extensions.Email.Abstractions.Models;
using Raycynix.Extensions.Email.Implementations;
using Raycynix.Extensions.Email.Smtp.Configurations;

namespace Raycynix.Extensions.Email.Smtp.Internal;

internal sealed class SmtpEmailSender(
    SmtpConfiguration smtpConfiguration,
    EmailProviderDescriptor providerDescriptor,
    IServiceProvider serviceProvider,
    SmtpMimeMessageFactory mimeMessageFactory) : IEmailSender
{
    private const string ProviderName = "smtp";

    public async Task<EmailSendResult> SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        providerDescriptor.Registration.Validate(serviceProvider);
        EnsureActiveProvider();
        message.Validate();

        var mimeMessage = await mimeMessageFactory.CreateAsync(message, cancellationToken);
        using var client = new SmtpClient();
        client.Timeout = smtpConfiguration.TimeoutMilliseconds;

        try
        {
            await client.ConnectAsync(
                smtpConfiguration.Host ?? throw new EmailProviderConfigurationException("SMTP host is required."),
                smtpConfiguration.Port,
                SmtpSecureSocketOptionsMapper.Map(smtpConfiguration),
                cancellationToken);

            var credentials = SmtpCredentialFactory.Create(smtpConfiguration);
            if (credentials is not null)
            {
                await client.AuthenticateAsync(credentials, cancellationToken);
            }

            await client.SendAsync(mimeMessage, cancellationToken);
            await client.DisconnectAsync(quit: true, cancellationToken);

            return EmailSendResult.Success(ProviderName);
        }
        catch (SmtpCommandException exception)
        {
            return EmailSendResult.Failure(
                ProviderName,
                exception.Message,
                exception.StatusCode.ToString());
        }
        catch (Exception exception) when (exception is SmtpProtocolException or IOException or SocketException or AuthenticationException)
        {
            throw new EmailSendException("SMTP provider could not complete the email send operation.", exception);
        }
    }

    private void EnsureActiveProvider()
    {
        if (!string.Equals(providerDescriptor.ProviderName, ProviderName, StringComparison.OrdinalIgnoreCase))
        {
            throw new EmailProviderConfigurationException(
                $"The active email provider is '{providerDescriptor.ProviderName}', but SMTP sender was resolved.");
        }
    }

}

using System.Net.Sockets;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Email.Abstractions.Exceptions;
using Raycynix.Extensions.Email.Abstractions.Interfaces;
using Raycynix.Extensions.Email.Abstractions.Models;
using Raycynix.Extensions.Email.Implementations;
using Raycynix.Extensions.Email.Smtp.Options;

namespace Raycynix.Extensions.Email.Smtp.Internal;

internal sealed class SmtpEmailSender(
    SmtpOptions smtpOptions,
    EmailProviderDescriptor providerDescriptor,
    IServiceProvider serviceProvider,
    SmtpMimeMessageFactory mimeMessageFactory,
    ILogger<SmtpEmailSender>? logger = null) : IEmailSender
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

        logger?.LogDebug(
            "Preparing SMTP email send. ToCount={ToCount}, CcCount={CcCount}, BccCount={BccCount}, AttachmentCount={AttachmentCount}, HeaderCount={HeaderCount}.",
            message.To.Count,
            message.Cc.Count,
            message.Bcc.Count,
            message.Attachments.Count,
            message.Headers.Count);

        using var mimeMessage = await mimeMessageFactory.CreateAsync(message, cancellationToken);
        using var client = new SmtpClient();
        client.Timeout = smtpOptions.TimeoutMilliseconds;

        try
        {
            var secureSocketOptions = SmtpSecureSocketOptionsMapper.Map(smtpOptions);

            logger?.LogDebug(
                "Connecting to SMTP server. Host={Host}, Port={Port}, SecureSocketOptions={SecureSocketOptions}, TimeoutMilliseconds={TimeoutMilliseconds}.",
                smtpOptions.Host,
                smtpOptions.Port,
                secureSocketOptions,
                smtpOptions.TimeoutMilliseconds);

            await client.ConnectAsync(
                smtpOptions.Host ?? throw new EmailProviderConfigurationException("SMTP host is required."),
                smtpOptions.Port,
                secureSocketOptions,
                cancellationToken);

            var credentials = SmtpCredentialFactory.Create(smtpOptions);
            if (credentials is not null)
            {
                logger?.LogDebug(
                    "Authenticating SMTP client. UsesDefaultCredentials={UsesDefaultCredentials}.",
                    smtpOptions.UseDefaultCredentials);

                await client.AuthenticateAsync(credentials, cancellationToken);
            }
            else
            {
                logger?.LogDebug("SMTP client authentication skipped because no credentials are configured.");
            }

            logger?.LogDebug("Sending SMTP email message.");
            await client.SendAsync(mimeMessage, cancellationToken);
            logger?.LogDebug("Disconnecting SMTP client after successful send.");
            await client.DisconnectAsync(quit: true, cancellationToken);

            logger?.LogInformation("SMTP email message sent successfully.");
            return EmailSendResult.Success(ProviderName);
        }
        catch (SmtpCommandException exception)
        {
            logger?.LogWarning(
                exception,
                "SMTP server rejected the email send operation. StatusCode={StatusCode}.",
                exception.StatusCode);

            return EmailSendResult.Failure(
                ProviderName,
                exception.Message,
                exception.StatusCode.ToString());
        }
        catch (Exception exception) when (exception is SmtpProtocolException or IOException or SocketException or AuthenticationException)
        {
            logger?.LogError(exception, "SMTP provider could not complete the email send operation.");

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

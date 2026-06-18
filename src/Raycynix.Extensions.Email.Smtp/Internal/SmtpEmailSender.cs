using System.Net.Sockets;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Raycynix.Extensions.Email.Abstractions.Enums;
using Raycynix.Extensions.Email.Abstractions.Exceptions;
using Raycynix.Extensions.Email.Abstractions.Interfaces;
using Raycynix.Extensions.Email.Abstractions.Models;
using Raycynix.Extensions.Email.Configurations;
using Raycynix.Extensions.Email.Implementations;
using Raycynix.Extensions.Email.Smtp.Configurations;
using Raycynix.Extensions.Email.Smtp.Enums;

namespace Raycynix.Extensions.Email.Smtp.Internal;

internal sealed class SmtpEmailSender(
    EmailConfiguration emailConfiguration,
    SmtpConfiguration smtpConfiguration,
    EmailProviderDescriptor providerDescriptor,
    IServiceProvider serviceProvider) : IEmailSender
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

        var mimeMessage = await CreateMimeMessageAsync(message, cancellationToken);
        using var client = new SmtpClient();
        client.Timeout = smtpConfiguration.TimeoutMilliseconds;

        try
        {
            await client.ConnectAsync(
                smtpConfiguration.Host ?? throw new EmailProviderConfigurationException("SMTP host is required."),
                smtpConfiguration.Port,
                ResolveSecureSocketOptions(),
                cancellationToken);

            if (!smtpConfiguration.UseDefaultCredentials &&
                !string.IsNullOrWhiteSpace(smtpConfiguration.Username))
            {
                await client.AuthenticateAsync(
                    smtpConfiguration.Username,
                    smtpConfiguration.Password ?? string.Empty,
                    cancellationToken);
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
        catch (Exception exception) when (exception is SmtpProtocolException or IOException or SocketException or MailKit.Security.AuthenticationException)
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

    private async Task<MimeMessage> CreateMimeMessageAsync(
        EmailMessage message,
        CancellationToken cancellationToken)
    {
        var sender = message.From
                     ?? emailConfiguration.ResolveDefaultFrom()
                     ?? throw new EmailSendException(
                         "Email message requires a sender address. Set EmailConfiguration.DefaultFromAddress or EmailMessage.From.");

        var mimeMessage = new MimeMessage
        {
            Subject = message.Subject
        };

        mimeMessage.From.Add(ToMailboxAddress(sender));
        AddRecipients(mimeMessage.To, message.To);
        AddRecipients(mimeMessage.Cc, message.Cc);
        AddRecipients(mimeMessage.Bcc, message.Bcc);
        AddReplyTo(mimeMessage, message);
        AddHeaders(mimeMessage, message.Headers);

        var bodyBuilder = new BodyBuilder();
        AddBody(bodyBuilder, message.Body);

        foreach (var attachment in message.Attachments)
        {
            await AddAttachmentAsync(bodyBuilder, attachment, cancellationToken);
        }

        mimeMessage.Body = bodyBuilder.ToMessageBody();
        return mimeMessage;
    }

    private void AddReplyTo(MimeMessage mimeMessage, EmailMessage message)
    {
        var replyTo = message.ReplyTo ?? emailConfiguration.ResolveDefaultReplyTo();
        if (replyTo is not null)
        {
            mimeMessage.ReplyTo.Add(ToMailboxAddress(replyTo));
        }
    }

    private static void AddBody(BodyBuilder bodyBuilder, EmailBody body)
    {
        if (!string.IsNullOrWhiteSpace(body.Html))
        {
            bodyBuilder.HtmlBody = body.Html;
        }

        if (!string.IsNullOrWhiteSpace(body.PlainText))
        {
            bodyBuilder.TextBody = body.PlainText;
        }

        if (body.PreferredFormat == EmailBodyFormat.PlainText &&
            !string.IsNullOrWhiteSpace(body.PlainText) &&
            string.IsNullOrWhiteSpace(body.Html))
        {
            bodyBuilder.TextBody = body.PlainText;
        }
    }

    private static async Task AddAttachmentAsync(
        BodyBuilder bodyBuilder,
        EmailAttachment attachment,
        CancellationToken cancellationToken)
    {
        await using var stream = await attachment.OpenReadAsync(cancellationToken);
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);

        var entity = bodyBuilder.Attachments.Add(
            attachment.FileName,
            memory.ToArray(),
            ContentType.Parse(attachment.ContentType));

        if (!string.IsNullOrWhiteSpace(attachment.ContentId) &&
            entity is MimePart mimePart)
        {
            mimePart.ContentId = attachment.ContentId;
        }
    }

    private static void AddHeaders(MimeMessage mimeMessage, IReadOnlyDictionary<string, string> headers)
    {
        foreach (var (key, value) in headers)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            mimeMessage.Headers.Replace(key, value);
        }
    }

    private static void AddRecipients(
        InternetAddressList target,
        IEnumerable<EmailAddress> recipients)
    {
        foreach (var recipient in recipients)
        {
            target.Add(ToMailboxAddress(recipient));
        }
    }

    private SecureSocketOptions ResolveSecureSocketOptions()
    {
        return smtpConfiguration.SecureSocketOptions switch
        {
            SmtpSecureSocketOptions.None => SecureSocketOptions.None,
            SmtpSecureSocketOptions.StartTls => SecureSocketOptions.StartTls,
            SmtpSecureSocketOptions.StartTlsWhenAvailable => SecureSocketOptions.StartTlsWhenAvailable,
            SmtpSecureSocketOptions.SslOnConnect => SecureSocketOptions.SslOnConnect,
            _ when smtpConfiguration.Port == 465 => SecureSocketOptions.SslOnConnect,
            _ when smtpConfiguration.EnableSsl => SecureSocketOptions.StartTls,
            _ => SecureSocketOptions.None
        };
    }

    private static MailboxAddress ToMailboxAddress(EmailAddress address)
    {
        return string.IsNullOrWhiteSpace(address.DisplayName)
            ? MailboxAddress.Parse(address.Address)
            : new MailboxAddress(address.DisplayName, address.Address);
    }
}

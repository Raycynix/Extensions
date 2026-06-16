using System.Net;
using System.Net.Mail;
using System.Text;
using Raycynix.Extensions.Email.Abstractions.Enums;
using Raycynix.Extensions.Email.Abstractions.Exceptions;
using Raycynix.Extensions.Email.Abstractions.Interfaces;
using Raycynix.Extensions.Email.Abstractions.Models;
using Raycynix.Extensions.Email.Configurations;
using Raycynix.Extensions.Email.Implementations;
using Raycynix.Extensions.Email.Smtp.Configurations;

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

        using var mailMessage = await CreateMailMessageAsync(message, cancellationToken);
        using var client = CreateClient();

        try
        {
            await client.SendMailAsync(mailMessage, cancellationToken);
            return EmailSendResult.Success(ProviderName);
        }
        catch (SmtpFailedRecipientsException exception)
        {
            return EmailSendResult.Failure(
                ProviderName,
                exception.Message,
                exception.StatusCode.ToString());
        }
        catch (SmtpFailedRecipientException exception)
        {
            return EmailSendResult.Failure(
                ProviderName,
                exception.Message,
                exception.StatusCode.ToString());
        }
        catch (SmtpException exception)
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

    private SmtpClient CreateClient()
    {
        var client = new SmtpClient(smtpConfiguration.Host, smtpConfiguration.Port)
        {
            EnableSsl = smtpConfiguration.EnableSsl,
            UseDefaultCredentials = smtpConfiguration.UseDefaultCredentials,
            Timeout = smtpConfiguration.TimeoutMilliseconds
        };

        if (!smtpConfiguration.UseDefaultCredentials &&
            !string.IsNullOrWhiteSpace(smtpConfiguration.Username))
        {
            client.Credentials = new NetworkCredential(
                smtpConfiguration.Username,
                smtpConfiguration.Password);
        }

        return client;
    }

    private async Task<MailMessage> CreateMailMessageAsync(
        EmailMessage message,
        CancellationToken cancellationToken)
    {
        var sender = message.From
                     ?? emailConfiguration.ResolveDefaultFrom()
                     ?? throw new EmailSendException(
                         "Email message requires a sender address. Set EmailConfiguration.DefaultFromAddress or EmailMessage.From.");

        var mailMessage = new MailMessage
        {
            From = ToMailAddress(sender),
            Subject = message.Subject,
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8,
            HeadersEncoding = Encoding.UTF8
        };

        AddRecipients(mailMessage.To, message.To);
        AddRecipients(mailMessage.CC, message.Cc);
        AddRecipients(mailMessage.Bcc, message.Bcc);
        AddReplyTo(mailMessage, message);
        AddBody(mailMessage, message.Body);
        AddHeaders(mailMessage, message.Headers);

        foreach (var attachment in message.Attachments)
        {
            var stream = await attachment.OpenReadAsync(cancellationToken);
            var mailAttachment = new Attachment(stream, attachment.FileName, attachment.ContentType);
            if (!string.IsNullOrWhiteSpace(attachment.ContentId))
            {
                mailAttachment.ContentId = attachment.ContentId;
            }

            mailMessage.Attachments.Add(mailAttachment);
        }

        return mailMessage;
    }

    private void AddReplyTo(MailMessage mailMessage, EmailMessage message)
    {
        var replyTo = message.ReplyTo ?? emailConfiguration.ResolveDefaultReplyTo();
        if (replyTo is not null)
        {
            mailMessage.ReplyToList.Add(ToMailAddress(replyTo));
        }
    }

    private static void AddBody(MailMessage mailMessage, EmailBody body)
    {
        if (!string.IsNullOrWhiteSpace(body.PlainText) &&
            !string.IsNullOrWhiteSpace(body.Html))
        {
            mailMessage.Body = body.PreferredFormat == EmailBodyFormat.PlainText
                ? body.PlainText
                : body.Html;
            mailMessage.IsBodyHtml = body.PreferredFormat == EmailBodyFormat.Html;
            mailMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
                body.PlainText,
                Encoding.UTF8,
                "text/plain"));
            mailMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
                body.Html,
                Encoding.UTF8,
                "text/html"));
            return;
        }

        if (!string.IsNullOrWhiteSpace(body.Html))
        {
            mailMessage.Body = body.Html;
            mailMessage.IsBodyHtml = true;
            return;
        }

        mailMessage.Body = body.PlainText ?? string.Empty;
        mailMessage.IsBodyHtml = false;
    }

    private static void AddHeaders(MailMessage mailMessage, IReadOnlyDictionary<string, string> headers)
    {
        foreach (var (key, value) in headers)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            mailMessage.Headers[key] = value;
        }
    }

    private static void AddRecipients(
        MailAddressCollection target,
        IEnumerable<EmailAddress> recipients)
    {
        foreach (var recipient in recipients)
        {
            target.Add(ToMailAddress(recipient));
        }
    }

    private static MailAddress ToMailAddress(EmailAddress address)
    {
        return string.IsNullOrWhiteSpace(address.DisplayName)
            ? new MailAddress(address.Address)
            : new MailAddress(address.Address, address.DisplayName, Encoding.UTF8);
    }
}

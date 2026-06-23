using MimeKit;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Email.Abstractions.Enums;
using Raycynix.Extensions.Email.Abstractions.Exceptions;
using Raycynix.Extensions.Email.Abstractions.Models;
using Raycynix.Extensions.Email.Configurations;

namespace Raycynix.Extensions.Email.Smtp.Internal;

internal sealed class SmtpMimeMessageFactory(
    EmailConfiguration emailConfiguration,
    ILogger<SmtpMimeMessageFactory>? logger = null)
{
    public async Task<MimeMessage> CreateAsync(
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

        var linkedResourceCount = 0;
        var attachmentCount = 0;
        foreach (var attachment in message.Attachments)
        {
            await AddAttachmentAsync(bodyBuilder, attachment, cancellationToken);

            if (string.IsNullOrWhiteSpace(attachment.ContentId))
            {
                attachmentCount++;
            }
            else
            {
                linkedResourceCount++;
            }
        }

        mimeMessage.Body = bodyBuilder.ToMessageBody();
        logger?.LogDebug(
            "Created SMTP MIME message. BodyFormat={BodyFormat}, ToCount={ToCount}, CcCount={CcCount}, BccCount={BccCount}, AttachmentCount={AttachmentCount}, LinkedResourceCount={LinkedResourceCount}, HeaderCount={HeaderCount}.",
            message.Body.PreferredFormat,
            message.To.Count,
            message.Cc.Count,
            message.Bcc.Count,
            attachmentCount,
            linkedResourceCount,
            message.Headers.Count);

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

        var contentType = ContentType.Parse(attachment.ContentType);
        var content = memory.ToArray();

        if (!string.IsNullOrWhiteSpace(attachment.ContentId))
        {
            var linkedResource = bodyBuilder.LinkedResources.Add(
                attachment.FileName,
                content,
                contentType);

            if (linkedResource is MimePart linkedResourcePart)
            {
                linkedResourcePart.ContentId = attachment.ContentId;
            }

            return;
        }

        bodyBuilder.Attachments.Add(
            attachment.FileName,
            content,
            contentType);
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

    private static MailboxAddress ToMailboxAddress(EmailAddress address)
    {
        return string.IsNullOrWhiteSpace(address.DisplayName)
            ? MailboxAddress.Parse(address.Address)
            : new MailboxAddress(address.DisplayName, address.Address);
    }
}

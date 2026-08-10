using MimeKit;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Email.Abstractions.Enums;
using Raycynix.Extensions.Email.Abstractions.Exceptions;
using Raycynix.Extensions.Email.Abstractions.Models;
using Raycynix.Extensions.Email.Options;

namespace Raycynix.Extensions.Email.Smtp.Internal;

internal sealed class SmtpMimeMessageFactory(
    EmailOptions emailConfiguration,
    ILogger<SmtpMimeMessageFactory>? logger = null)
{
    public async Task<MimeMessage> CreateAsync(
        EmailMessage message,
        CancellationToken cancellationToken)
    {
        var sender = message.From
                     ?? emailConfiguration.ResolveDefaultFrom()
                     ?? throw new EmailSendException(
                         "Email message requires a sender address. Set EmailOptions.DefaultFromAddress or EmailMessage.From.");

        var mimeMessage = new StreamOwningMimeMessage
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
        var attachmentStreams = new List<Stream>(message.Attachments.Count);

        try
        {
            foreach (var attachment in message.Attachments)
            {
                attachmentStreams.Add(await AddAttachmentAsync(bodyBuilder, attachment, cancellationToken));

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
            mimeMessage.TakeOwnership(attachmentStreams);
        }
        catch
        {
            foreach (var stream in attachmentStreams)
            {
                await stream.DisposeAsync();
            }

            mimeMessage.Dispose();
            throw;
        }

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

    private static async Task<Stream> AddAttachmentAsync(
        BodyBuilder bodyBuilder,
        EmailAttachment attachment,
        CancellationToken cancellationToken)
    {
        var stream = await attachment.OpenReadAsync(cancellationToken);
        var contentType = ContentType.Parse(attachment.ContentType);
        try
        {
            if (!string.IsNullOrWhiteSpace(attachment.ContentId))
            {
                var linkedResource = bodyBuilder.LinkedResources.Add(
                    attachment.FileName,
                    stream,
                    contentType,
                    cancellationToken);

                if (linkedResource is MimePart linkedResourcePart)
                {
                    linkedResourcePart.ContentId = attachment.ContentId;
                }

                return stream;
            }

            bodyBuilder.Attachments.Add(
                attachment.FileName,
                stream,
                contentType,
                cancellationToken);

            return stream;
        }
        catch
        {
            await stream.DisposeAsync();
            throw;
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

    private static MailboxAddress ToMailboxAddress(EmailAddress address)
    {
        return string.IsNullOrWhiteSpace(address.DisplayName)
            ? MailboxAddress.Parse(address.Address)
            : new MailboxAddress(address.DisplayName, address.Address);
    }

    private sealed class StreamOwningMimeMessage : MimeMessage
    {
        private IReadOnlyCollection<Stream>? _ownedStreams;

        public void TakeOwnership(IReadOnlyCollection<Stream> streams)
        {
            _ownedStreams = streams;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _ownedStreams is not null)
            {
                foreach (var stream in _ownedStreams)
                {
                    stream.Dispose();
                }

                _ownedStreams = null;
            }

            base.Dispose(disposing);
        }
    }
}

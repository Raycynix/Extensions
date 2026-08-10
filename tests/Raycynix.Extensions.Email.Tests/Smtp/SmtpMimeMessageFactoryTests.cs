using FluentAssertions;
using MimeKit;
using Raycynix.Extensions.Email.Abstractions.Models;
using Raycynix.Extensions.Email.Options;
using Raycynix.Extensions.Email.Smtp.Internal;

namespace Raycynix.Extensions.Email.Tests.Smtp;

/// <summary>
/// Covers SMTP MIME message construction.
/// </summary>
public sealed class SmtpMimeMessageFactoryTests
{
    /// <summary>
    /// Verifies that attachments with content ids become linked resources for HTML cid references.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldPutContentIdAttachmentsIntoLinkedResources()
    {
        var factory = new SmtpMimeMessageFactory(new EmailOptions
        {
            DefaultFromAddress = "sender@example.com"
        });
        var message = new EmailMessage
        {
            To = [new EmailAddress("recipient@example.com")],
            Subject = "Inline image",
            Body = EmailBody.FromHtml(
                "<html><body><img src=\"cid:logo\"></body></html>",
                "Inline image"),
            Attachments =
            [
                EmailAttachment.FromBytes(
                    "logo.png",
                    [1, 2, 3],
                    "image/png",
                    "logo"),
                EmailAttachment.FromBytes(
                    "document.txt",
                    [4, 5, 6],
                    "text/plain")
            ]
        };

        using var mimeMessage = await factory.CreateAsync(message, TestContext.Current.CancellationToken);

        mimeMessage.Body.Should().NotBeNull();
        var related = EnumerateMimeEntities(mimeMessage.Body!)
            .OfType<MultipartRelated>()
            .Single();
        var linkedResource = related
            .OfType<MimePart>()
            .Single(part => part.ContentId == "logo");
        var attachments = mimeMessage.Attachments
            .OfType<MimePart>()
            .ToArray();

        linkedResource.FileName.Should().Be("logo.png");
        linkedResource.ContentType.MimeType.Should().Be("image/png");
        attachments.Should().ContainSingle(part => part.FileName == "document.txt");
        attachments.Should().NotContain(part => part.ContentId == "logo");
    }

    [Fact]
    public async Task CreateAsync_ShouldKeepAttachmentStreamUntilMessageIsDisposed()
    {
        var factory = new SmtpMimeMessageFactory(new EmailOptions
        {
            DefaultFromAddress = "sender@example.com"
        });
        var stream = new TrackingMemoryStream([1, 2, 3]);
        var message = new EmailMessage
        {
            To = [new EmailAddress("recipient@example.com")],
            Subject = "Streamed attachment",
            Body = EmailBody.FromPlainText("Body"),
            Attachments =
            [
                new EmailAttachment
                {
                    FileName = "data.bin",
                    OpenReadAsync = _ => ValueTask.FromResult<Stream>(stream)
                }
            ]
        };

        var mimeMessage = await factory.CreateAsync(message, TestContext.Current.CancellationToken);

        stream.IsDisposed.Should().BeFalse();
        mimeMessage.Dispose();
        stream.IsDisposed.Should().BeTrue();
    }

    private sealed class TrackingMemoryStream(byte[] content) : MemoryStream(content)
    {
        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }

    private static IEnumerable<MimeEntity> EnumerateMimeEntities(MimeEntity entity)
    {
        yield return entity;

        if (entity is not Multipart multipart)
        {
            yield break;
        }

        foreach (var child in multipart.SelectMany(EnumerateMimeEntities))
        {
            yield return child;
        }
    }
}

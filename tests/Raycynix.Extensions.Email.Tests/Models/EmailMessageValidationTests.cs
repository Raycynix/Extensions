using FluentAssertions;
using Raycynix.Extensions.Email.Abstractions.Models;

namespace Raycynix.Extensions.Email.Tests.Models;

/// <summary>
/// Covers outgoing email message validation.
/// </summary>
public sealed class EmailMessageValidationTests
{
    /// <summary>
    /// Verifies that a plain text message with only primary recipients is valid.
    /// </summary>
    [Fact]
    public void Validate_ShouldPass_ForPlainTextMessageWithoutOptionalCollections()
    {
        var message = new EmailMessage
        {
            To = [new EmailAddress("user@example.com")],
            Subject = "Hello",
            Body = EmailBody.FromPlainText("Hello")
        };

        var act = message.Validate;

        act.Should().NotThrow();
        message.HasRecipients.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that a message must have at least one recipient across To, Cc, and Bcc.
    /// </summary>
    [Fact]
    public void Validate_ShouldThrow_WhenMessageHasNoRecipients()
    {
        var message = new EmailMessage
        {
            Subject = "Hello",
            Body = EmailBody.FromPlainText("Hello")
        };

        var act = message.Validate;

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Email message requires at least one recipient.");
    }

    /// <summary>
    /// Verifies that optional collections may be empty but cannot be null.
    /// </summary>
    [Fact]
    public void Validate_ShouldThrow_WhenAttachmentsCollectionIsNull()
    {
        var message = new EmailMessage
        {
            To = [new EmailAddress("user@example.com")],
            Subject = "Hello",
            Body = EmailBody.FromPlainText("Hello"),
            Attachments = null!
        };

        var act = message.Validate;

        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that collection entries cannot contain null values.
    /// </summary>
    [Fact]
    public void Validate_ShouldThrow_WhenRecipientCollectionContainsNull()
    {
        var message = new EmailMessage
        {
            To = [null!],
            Subject = "Hello",
            Body = EmailBody.FromPlainText("Hello")
        };

        var act = message.Validate;

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("To cannot contain null values.");
    }

    /// <summary>
    /// Verifies that header names must be explicit before provider-specific MIME creation.
    /// </summary>
    [Fact]
    public void Validate_ShouldThrow_WhenHeaderKeyIsEmpty()
    {
        var message = new EmailMessage
        {
            To = [new EmailAddress("user@example.com")],
            Subject = "Hello",
            Body = EmailBody.FromPlainText("Hello"),
            Headers = new Dictionary<string, string>
            {
                [" "] = "value"
            }
        };

        var act = message.Validate;

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Headers cannot contain null or whitespace keys.");
    }

    /// <summary>
    /// Verifies that metadata values cannot contain null entries.
    /// </summary>
    [Fact]
    public void Validate_ShouldThrow_WhenMetadataValueIsNull()
    {
        var message = new EmailMessage
        {
            To = [new EmailAddress("user@example.com")],
            Subject = "Hello",
            Body = EmailBody.FromPlainText("Hello"),
            Metadata = new Dictionary<string, string>
            {
                ["trace"] = null!
            }
        };

        var act = message.Validate;

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Metadata cannot contain null values.");
    }

    /// <summary>
    /// Verifies that attachment validation runs during message validation.
    /// </summary>
    [Fact]
    public void Validate_ShouldThrow_WhenAttachmentContentTypeIsInvalid()
    {
        var message = new EmailMessage
        {
            To = [new EmailAddress("user@example.com")],
            Subject = "Hello",
            Body = EmailBody.FromPlainText("Hello"),
            Attachments =
            [
                new EmailAttachment
                {
                    FileName = "data.bin",
                    ContentType = "not a content type",
                    OpenReadAsync = _ => ValueTask.FromResult<Stream>(new MemoryStream([1, 2, 3]))
                }
            ]
        };

        var act = message.Validate;

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Attachment content type must be a valid MIME content type.*");
    }
}

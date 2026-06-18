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
}

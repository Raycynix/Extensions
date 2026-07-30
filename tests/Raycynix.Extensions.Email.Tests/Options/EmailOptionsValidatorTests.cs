using FluentAssertions;
using Raycynix.Extensions.Email.Options;
using Raycynix.Extensions.Email.Internal;

namespace Raycynix.Extensions.Email.Tests.Options;

/// <summary>
/// Covers shared email configuration validation.
/// </summary>
public sealed class EmailOptionsValidatorTests
{
    /// <summary>
    /// Verifies that valid default sender configuration is accepted.
    /// </summary>
    [Fact]
    public void Validate_ShouldSucceed_WhenDefaultsAreValid()
    {
        var validator = new EmailOptionsValidator();
        var configuration = new EmailOptions
        {
            DefaultFromAddress = "sender@example.com",
            DefaultFromDisplayName = "Sender",
            DefaultReplyToAddress = "reply@example.com",
            DefaultReplyToDisplayName = "Reply"
        };

        var result = validator.Validate(configuration);

        result.Succeeded.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that default display names are validated before the first send.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Validate_ShouldFail_WhenDefaultDisplayNameIsTooLong(bool fromDisplayName)
    {
        var validator = new EmailOptionsValidator();
        var displayName = new string('a', 256);
        var configuration = new EmailOptions
        {
            DefaultFromAddress = "sender@example.com",
            DefaultReplyToAddress = "reply@example.com",
            DefaultFromDisplayName = fromDisplayName ? displayName : null,
            DefaultReplyToDisplayName = fromDisplayName ? null : displayName
        };

        var result = validator.Validate(configuration);

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle(error => error.Contains("cannot exceed 255 characters."));
    }

    /// <summary>
    /// Verifies that configured default addresses use the same strict mailbox validation as EmailAddress.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Validate_ShouldFail_WhenDefaultAddressContainsDisplayNameMailbox(bool fromAddress)
    {
        var validator = new EmailOptionsValidator();
        var configuration = new EmailOptions
        {
            DefaultFromAddress = fromAddress ? "Sender <sender@example.com>" : "sender@example.com",
            DefaultReplyToAddress = fromAddress ? "reply@example.com" : "Reply <reply@example.com>"
        };

        var result = validator.Validate(configuration);

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle(error => error.Contains("must be a valid email address."));
    }

    /// <summary>
    /// Verifies that display names cannot be configured without their corresponding addresses.
    /// </summary>
    [Fact]
    public void Validate_ShouldFail_WhenDisplayNameHasNoAddress()
    {
        var validator = new EmailOptionsValidator();
        var options = new EmailOptions
        {
            DefaultFromDisplayName = "Sender"
        };

        var result = validator.Validate(options);

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle(
            error => error == "DefaultFromDisplayName requires DefaultFromAddress.");
    }
}

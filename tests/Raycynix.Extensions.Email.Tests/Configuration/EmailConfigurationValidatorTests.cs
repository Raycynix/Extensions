using FluentAssertions;
using Raycynix.Extensions.Email.Configurations;
using Raycynix.Extensions.Email.Internal;

namespace Raycynix.Extensions.Email.Tests.Configuration;

/// <summary>
/// Covers shared email configuration validation.
/// </summary>
public sealed class EmailConfigurationValidatorTests
{
    /// <summary>
    /// Verifies that valid default sender configuration is accepted.
    /// </summary>
    [Fact]
    public void Validate_ShouldSucceed_WhenDefaultsAreValid()
    {
        var validator = new EmailConfigurationValidator();
        var configuration = new EmailConfiguration
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
        var validator = new EmailConfigurationValidator();
        var displayName = new string('a', 256);
        var configuration = new EmailConfiguration
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
        var validator = new EmailConfigurationValidator();
        var configuration = new EmailConfiguration
        {
            DefaultFromAddress = fromAddress ? "Sender <sender@example.com>" : "sender@example.com",
            DefaultReplyToAddress = fromAddress ? "reply@example.com" : "Reply <reply@example.com>"
        };

        var result = validator.Validate(configuration);

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle(error => error.Contains("must be a valid email address."));
    }
}

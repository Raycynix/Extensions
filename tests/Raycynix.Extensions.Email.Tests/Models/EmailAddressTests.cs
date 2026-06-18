using FluentAssertions;
using Raycynix.Extensions.Email.Abstractions.Models;

namespace Raycynix.Extensions.Email.Tests.Models;

/// <summary>
/// Covers email address invariants.
/// </summary>
public sealed class EmailAddressTests
{
    /// <summary>
    /// Verifies that a valid mailbox address is accepted.
    /// </summary>
    [Fact]
    public void Constructor_ShouldCreateAddress_WhenAddressIsValid()
    {
        var address = new EmailAddress("user@example.com", "User");

        address.Address.Should().Be("user@example.com");
        address.DisplayName.Should().Be("User");
    }

    /// <summary>
    /// Verifies that invalid mailbox addresses are rejected at construction time.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("User <user@example.com>")]
    public void Constructor_ShouldThrow_WhenAddressIsInvalid(string value)
    {
        var act = () => new EmailAddress(value);

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that display names are bounded.
    /// </summary>
    [Fact]
    public void Constructor_ShouldThrow_WhenDisplayNameIsTooLong()
    {
        var displayName = new string('a', 256);

        var act = () => new EmailAddress("user@example.com", displayName);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}

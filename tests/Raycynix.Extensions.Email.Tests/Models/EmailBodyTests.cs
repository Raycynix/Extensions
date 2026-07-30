using FluentAssertions;
using Raycynix.Extensions.Email.Abstractions.Models;

namespace Raycynix.Extensions.Email.Tests.Models;

/// <summary>
/// Covers email body factory invariants.
/// </summary>
public sealed class EmailBodyTests
{
    /// <summary>
    /// Verifies that an explicitly supplied plain-text alternative contains sendable content.
    /// </summary>
    [Fact]
    public void FromHtml_ShouldThrow_WhenPlainTextAlternativeIsWhitespace()
    {
        var act = () => EmailBody.FromHtml("<p>Hello</p>", " ");

        act.Should().Throw<ArgumentException>();
    }
}

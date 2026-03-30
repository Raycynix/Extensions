using FluentAssertions;
using Raycynix.Extensions.Contracts.Models;

namespace Raycynix.Extensions.Contracts.Tests.Models;

/// <summary>
/// Covers semantic version parsing and ordering for shared contracts.
/// </summary>
public sealed class ContractVersionTests
{
    /// <summary>
    /// Verifies that semantic version strings are parsed into version components.
    /// </summary>
    [Fact]
    public void Parse_ShouldCreateVersion_WhenValueIsValid()
    {
        var version = ContractVersion.Parse("2.5.9");

        version.Major.Should().Be(2);
        version.Minor.Should().Be(5);
        version.Patch.Should().Be(9);
        version.ToString().Should().Be("2.5.9");
    }

    /// <summary>
    /// Verifies that invalid semantic version strings are rejected.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1")]
    [InlineData("1.2")]
    [InlineData("1.2.-1")]
    [InlineData("v1.2.3")]
    public void TryParse_ShouldReturnFalse_WhenValueIsInvalid(string? value)
    {
        var parsed = ContractVersion.TryParse(value, out var version);

        parsed.Should().BeFalse();
        version.Should().BeNull();
    }

    /// <summary>
    /// Verifies that comparison operators follow semantic version ordering.
    /// </summary>
    [Fact]
    public void Operators_ShouldCompareVersionsByMajorMinorPatch()
    {
        var current = new ContractVersion { Major = 1, Minor = 2, Patch = 0 };
        var newerMinor = new ContractVersion { Major = 1, Minor = 3, Patch = 0 };
        var newerMajor = new ContractVersion { Major = 2, Minor = 0, Patch = 0 };

        (newerMinor > current).Should().BeTrue();
        (newerMajor > newerMinor).Should().BeTrue();
        (current < newerMajor).Should().BeTrue();
        (current == new ContractVersion { Major = 1, Minor = 2, Patch = 0 }).Should().BeTrue();
    }
}

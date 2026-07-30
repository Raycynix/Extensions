using FluentAssertions;
using Raycynix.Extensions.Contracts.Models;

namespace Raycynix.Extensions.Contracts.Tests.Models;

/// <summary>
/// Covers contract identity metadata behavior.
/// </summary>
public sealed class ContractMetadataTests
{
    /// <summary>
    /// Verifies that metadata identity requires both a name and a valid semantic version.
    /// </summary>
    [Fact]
    public void HasIdentity_ShouldBeTrue_WhenNameAndVersionAreValid()
    {
        var metadata = new ContractMetadata
        {
            Name = "catalog.prices",
            Version = new ContractVersion { Major = 1, Minor = 2, Patch = 0 }
        };

        metadata.HasIdentity.Should().BeTrue();
        metadata.ToString().Should().Be("catalog.prices:1.2.0");
    }

    /// <summary>
    /// Verifies that metadata identity fails when the version is invalid.
    /// </summary>
    [Fact]
    public void HasIdentity_ShouldBeFalse_WhenVersionIsInvalid()
    {
        var metadata = new ContractMetadata
        {
            Name = "catalog.prices",
            Version = new ContractVersion { Major = -1, Minor = 0, Patch = 0 }
        };

        metadata.HasIdentity.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that malformed deserialized metadata does not throw during identity checks.
    /// </summary>
    [Fact]
    public void HasIdentity_ShouldBeFalse_WhenVersionIsNull()
    {
        var metadata = new ContractMetadata
        {
            Name = "catalog.prices",
            Version = null!
        };

        metadata.HasIdentity.Should().BeFalse();
        metadata.ToString().Should().Be("catalog.prices");
    }
}

using FluentAssertions;
using Raycynix.Extensions.Contracts.Attributes;

namespace Raycynix.Extensions.Contracts.Tests.Models;

/// <summary>
/// Covers semantic version validation in contract evolution attributes.
/// </summary>
public sealed class ContractEvolutionAttributeTests
{
    /// <summary>
    /// Verifies that evolution attributes normalize valid semantic versions.
    /// </summary>
    [Fact]
    public void Attributes_ShouldAcceptValidSemanticVersions()
    {
        var introduced = new ContractIntroducedAttribute(" 1.2.0 ");
        var deprecated = new ContractDeprecatedAttribute("1.3.0")
        {
            RemovalVersion = "2.0.0"
        };

        introduced.Version.Should().Be("1.2.0");
        deprecated.DeprecatedSinceVersion.Should().Be("1.3.0");
        deprecated.RemovalVersion.Should().Be("2.0.0");
    }

    /// <summary>
    /// Verifies that invalid semantic versions are rejected when attributes are constructed.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("1.0")]
    [InlineData("v1.0.0")]
    public void Attributes_ShouldRejectInvalidSemanticVersions(string version)
    {
        var introduced = () => new ContractIntroducedAttribute(version);
        var deprecated = () => new ContractDeprecatedAttribute(version);

        introduced.Should().Throw<Exception>();
        deprecated.Should().Throw<Exception>();
    }
}

using FluentAssertions;

namespace Raycynix.Extensions.Configuration.Tests.TypedConfiguration;

/// <summary>
/// Covers conventional configuration paths derived from options types.
/// </summary>
public sealed class ConfigurationSectionPathTests
{
    [Fact]
    public void For_ShouldReturnOptionsTypeName()
    {
        ConfigurationSectionPath.For<RootOptions>()
            .Should().Be(nameof(RootOptions));
    }

    [Fact]
    public void Combine_ShouldReturnNestedOptionsTypePath()
    {
        ConfigurationSectionPath.Combine<RootOptions, NestedOptions>()
            .Should().Be($"{nameof(RootOptions)}:{nameof(NestedOptions)}");
    }

    private sealed class RootOptions;

    private sealed class NestedOptions;
}

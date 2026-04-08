using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;

namespace Raycynix.Extensions.Configuration.Tests.FeatureFlags;

/// <summary>
/// Covers feature flag accessor behavior.
/// </summary>
public class FeatureFlagAccessorTests
{
    /// <summary>
    /// Verifies that the standard <c>FeatureFlags</c> section is read correctly and flag lookup stays case-insensitive.
    /// </summary>
    [Fact]
    public void AddRaycynixFeatureFlags_ShouldReadFlagsFromStandardSection()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FeatureFlags:Flags:NewDashboard"] = "true",
                ["FeatureFlags:Flags:UseFastCache"] = "false"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddRaycynixFeatureFlags(configuration);

        using var provider = services.BuildServiceProvider();
        var featureFlags = provider.GetRequiredService<IFeatureFlagAccessor>();

        featureFlags.IsEnabled("NewDashboard").Should().BeTrue();
        featureFlags.IsDisabled("UseFastCache").Should().BeTrue();
        featureFlags.IsEnabled("newdashboard").Should().BeTrue();
        featureFlags.GetAll().Should().ContainKey("NewDashboard").WhoseValue.Should().BeTrue();
        featureFlags.GetAll().Should().ContainKey("UseFastCache").WhoseValue.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that feature flags can bind from an explicit custom section name.
    /// </summary>
    [Fact]
    public void AddRaycynixFeatureFlags_ShouldReadFlagsFromExplicitSection()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PlatformFlags:Flags:NewDashboard"] = "true",
                ["PlatformFlags:Flags:UseFastCache"] = "false"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddRaycynixFeatureFlags(configuration, sectionName: "PlatformFlags");

        using var provider = services.BuildServiceProvider();
        var featureFlags = provider.GetRequiredService<IFeatureFlagAccessor>();

        featureFlags.IsEnabled("NewDashboard").Should().BeTrue();
        featureFlags.IsDisabled("UseFastCache").Should().BeTrue();
    }
}

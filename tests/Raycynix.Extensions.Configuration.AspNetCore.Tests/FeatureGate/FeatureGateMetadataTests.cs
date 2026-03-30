using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Raycynix.Extensions.Configuration.AspNetCore.FeatureGate;

namespace Raycynix.Extensions.Configuration.AspNetCore.Tests.FeatureGate;

/// <summary>
/// Covers feature gate metadata, attribute, and guard behavior.
/// </summary>
public class FeatureGateMetadataTests
{
    /// <summary>
    /// Verifies that metadata normalization removes duplicates and whitespace-only values.
    /// </summary>
    [Fact]
    public void CreateAll_ShouldNormalizeFeatureFlags()
    {
        var metadata = FeatureGateMetadata.CreateAll("Alpha", "alpha", " ", "Beta");

        metadata.Mode.Should().Be(FeatureGateMode.All);
        metadata.FeatureFlags.Should().BeEquivalentTo(["Alpha", "Beta"]);
    }

    /// <summary>
    /// Verifies that metadata creation rejects an empty feature flag set.
    /// </summary>
    [Fact]
    public void CreateAny_ShouldRejectMissingFeatureFlags()
    {
        var action = () => FeatureGateMetadata.CreateAny("", " ", null!);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*At least one feature flag must be provided.*");
    }

    /// <summary>
    /// Verifies that the feature gate attribute preserves the requested mode and feature flags.
    /// </summary>
    [Fact]
    public void FeatureGateAttribute_ShouldCreateMetadataForRequestedMode()
    {
        var attribute = new FeatureGateAttribute(FeatureGateMode.Any, "Alpha", "Beta");
        var metadata = attribute.GetType()
            .GetProperty("Metadata", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(attribute)
            .As<FeatureGateMetadata>();

        metadata.Mode.Should().Be(FeatureGateMode.Any);
        metadata.FeatureFlags.Should().BeEquivalentTo(["Alpha", "Beta"]);
    }

    /// <summary>
    /// Verifies that <c>RequireFeature</c> rejects an empty feature flag set.
    /// </summary>
    [Fact]
    public void RequireFeature_ShouldRejectEmptyFeatureFlags()
    {
        var builder = WebApplication.CreateBuilder();
        using var app = builder.Build();
        var endpoint = app.MapGet("/feature", () => "ok");

        var action = () => endpoint.RequireFeature("", " ");

        action.Should().Throw<ArgumentException>()
            .WithMessage("*At least one feature flag must be provided.*");
    }

    /// <summary>
    /// Verifies that <c>RequireAnyFeature</c> rejects an empty feature flag set.
    /// </summary>
    [Fact]
    public void RequireAnyFeature_ShouldRejectEmptyFeatureFlags()
    {
        var builder = WebApplication.CreateBuilder();
        using var app = builder.Build();
        var endpoint = app.MapGet("/feature", () => "ok");

        var action = () => endpoint.RequireAnyFeature("", " ");

        action.Should().Throw<ArgumentException>()
            .WithMessage("*At least one feature flag must be provided.*");
    }
}

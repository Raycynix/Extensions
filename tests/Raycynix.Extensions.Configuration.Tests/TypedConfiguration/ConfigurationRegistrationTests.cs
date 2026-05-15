using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;

namespace Raycynix.Extensions.Configuration.Tests.TypedConfiguration;

/// <summary>
/// Covers typed configuration registration behavior.
/// </summary>
public class ConfigurationRegistrationTests
{
    /// <summary>
    /// Verifies that code defaults are applied before configuration binding and that the accessor exposes the final snapshot.
    /// </summary>
    [Fact]
    public void AddRaycynixConfiguration_ShouldApplyDefaultsBeforeBindingAndExposeFinalSnapshot()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SampleOptions:Value"] = "from-config"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddRaycynixConfiguration<SampleOptions>(
            configuration,
            configureDefaults: options =>
            {
                options.Value = "from-default";
                options.TimeoutSeconds = 30;
            });

        using var provider = services.BuildServiceProvider();
        var accessor = provider.GetRequiredService<IConfigurationAccessor<SampleOptions>>();

        accessor.Current.Value.Should().Be("from-config");
        accessor.Current.TimeoutSeconds.Should().Be(30);
    }

    /// <summary>
    /// Verifies that post-bind configuration runs after values have already been bound from configuration.
    /// </summary>
    [Fact]
    public void AddRaycynixConfiguration_ShouldApplyPostBindAfterConfigurationBinding()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PostBindOptions:TimeoutSeconds"] = "15"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddRaycynixConfiguration<PostBindOptions>(
            configuration,
            configurePostBind: options => options.TimeoutSeconds = 45);

        using var provider = services.BuildServiceProvider();
        var accessor = provider.GetRequiredService<IConfigurationAccessor<PostBindOptions>>();

        accessor.Current.TimeoutSeconds.Should().Be(45);
    }

    /// <summary>
    /// Verifies that an explicit section name can be used instead of the options type name.
    /// </summary>
    [Fact]
    public void AddRaycynixConfiguration_ShouldBindFromExplicitSectionName()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FeatureArea:Sample:Value"] = "from-custom-section",
                ["FeatureArea:Sample:TimeoutSeconds"] = "12"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddRaycynixConfiguration<SampleOptions>(
            configuration,
            sectionName: "FeatureArea:Sample");

        using var provider = services.BuildServiceProvider();
        var accessor = provider.GetRequiredService<IConfigurationAccessor<SampleOptions>>();

        accessor.Current.Value.Should().Be("from-custom-section");
        accessor.Current.TimeoutSeconds.Should().Be(12);
    }

    /// <summary>
    /// Verifies that a typed configuration model can be registered as a named options instance.
    /// </summary>
    [Fact]
    public void AddRaycynixConfiguration_ShouldBindNamedOptionsInstance()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Tenants:Primary:Value"] = "from-primary",
                ["Tenants:Secondary:Value"] = "from-secondary"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddRaycynixConfiguration<SampleOptions>(
            configuration,
            sectionName: "Tenants:Primary",
            optionsName: "Primary");
        services.AddRaycynixConfiguration<SampleOptions>(
            configuration,
            sectionName: "Tenants:Secondary",
            optionsName: "Secondary");

        using var provider = services.BuildServiceProvider();
        var accessor = provider.GetRequiredService<IConfigurationAccessor<SampleOptions>>();

        accessor.Get("Primary").Value.Should().Be("from-primary");
        accessor.Get("Secondary").Value.Should().Be("from-secondary");
    }

    /// <summary>
    /// Verifies that missing sections are rejected when section presence is required.
    /// </summary>
    [Fact]
    public void AddRaycynixConfiguration_ShouldThrow_WhenRequiredSectionIsMissing()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();

        var action = () => services.AddRaycynixConfiguration<SampleOptions>(
            configuration,
            requireSection: true);

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*Required configuration section 'SampleOptions'*");
    }

    /// <summary>
    /// Verifies that existing sections are accepted when section presence is required.
    /// </summary>
    [Fact]
    public void AddRaycynixConfiguration_ShouldSucceed_WhenRequiredSectionExists()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SampleOptions:Value"] = "from-required-section"
            })
            .Build();

        var services = new ServiceCollection();

        services.AddRaycynixConfiguration<SampleOptions>(
            configuration,
            requireSection: true);

        using var provider = services.BuildServiceProvider();
        var accessor = provider.GetRequiredService<IConfigurationAccessor<SampleOptions>>();

        accessor.Current.Value.Should().Be("from-required-section");
    }

    private class SampleOptions
    {
        public string Value { get; set; } = string.Empty;

        public int TimeoutSeconds { get; set; }
    }

    private class PostBindOptions
    {
        public int TimeoutSeconds { get; set; }
    }
}

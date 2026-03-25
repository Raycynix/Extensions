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

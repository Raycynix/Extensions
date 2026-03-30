using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;

namespace Raycynix.Extensions.Configuration.Tests.Accessor;

/// <summary>
/// Covers unified typed configuration access behavior.
/// </summary>
public class ConfigurationAccessorTests
{
    /// <summary>
    /// Verifies that the accessor can return a named options instance when the named options were registered externally.
    /// </summary>
    [Fact]
    public void AddRaycynixConfigurationAccessor_ShouldExposeNamedOptions()
    {
        var services = new ServiceCollection();
        services.AddOptions<NamedOptions>("primary").Configure(options => options.Value = "from-named-options");
        services.AddRaycynixConfigurationAccessor<NamedOptions>();

        using var provider = services.BuildServiceProvider();
        var accessor = provider.GetRequiredService<IConfigurationAccessor<NamedOptions>>();

        accessor.Get("primary").Value.Should().Be("from-named-options");
    }

    private class NamedOptions
    {
        public string Value { get; set; } = string.Empty;
    }
}

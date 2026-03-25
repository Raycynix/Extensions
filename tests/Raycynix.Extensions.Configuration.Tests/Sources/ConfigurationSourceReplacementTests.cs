using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;

namespace Raycynix.Extensions.Configuration.Tests.Sources;

/// <summary>
/// Covers differences between adding and replacing standard configuration sources.
/// </summary>
public class ConfigurationSourceReplacementTests
{
    /// <summary>
    /// Verifies that <c>UseRaycynixConfigurationSources</c> clears existing sources before applying the standard set.
    /// </summary>
    [Fact]
    public void UseRaycynixConfigurationSources_ShouldReplaceExistingSources()
    {
        var builder = new ConfigurationBuilder();
        builder.AddInMemoryCollection(new Dictionary<string, string?> { ["Existing:Value"] = "kept" });

        builder.UseRaycynixConfigurationSources(options =>
        {
            options.BasePath = AppContext.BaseDirectory;
            options.EnvironmentName = EnvironmentNames.Production;
            options.ReloadOnChange = false;
        });

        builder.Sources.Should().NotContain(source => source is MemoryConfigurationSource);
    }

    /// <summary>
    /// Verifies that <c>AddRaycynixConfigurationSources</c> appends the standard set without clearing existing sources.
    /// </summary>
    [Fact]
    public void AddRaycynixConfigurationSources_ShouldKeepExistingSources()
    {
        var builder = new ConfigurationBuilder();
        builder.AddInMemoryCollection(new Dictionary<string, string?> { ["Existing:Value"] = "kept" });

        builder.AddRaycynixConfigurationSources(options =>
        {
            options.BasePath = AppContext.BaseDirectory;
            options.EnvironmentName = EnvironmentNames.Production;
            options.ReloadOnChange = false;
        });

        builder.Sources.Should().Contain(source => source is MemoryConfigurationSource);
    }
}

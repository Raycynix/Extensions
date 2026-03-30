using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace Raycynix.Extensions.Configuration.Tests.Sources;

/// <summary>
/// Covers invalid source configuration guard behavior.
/// </summary>
public class ConfigurationSourceValidationTests
{
    /// <summary>
    /// Verifies that an empty base path is rejected.
    /// </summary>
    [Fact]
    public void AddRaycynixConfigurationSources_ShouldRejectEmptyBasePath()
    {
        var builder = new ConfigurationBuilder();

        var action = () => builder.AddRaycynixConfigurationSources(options => options.BasePath = string.Empty);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*Configuration base path cannot be null or whitespace.*");
    }

    /// <summary>
    /// Verifies that an empty environment name is rejected.
    /// </summary>
    [Fact]
    public void AddRaycynixConfigurationSources_ShouldRejectEmptyEnvironmentName()
    {
        var builder = new ConfigurationBuilder();

        var action = () => builder.AddRaycynixConfigurationSources(options => options.EnvironmentName = string.Empty);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*Environment name cannot be null or whitespace.*");
    }

    /// <summary>
    /// Verifies that an empty base file name is rejected.
    /// </summary>
    [Fact]
    public void AddRaycynixConfigurationSources_ShouldRejectEmptyBaseFileName()
    {
        var builder = new ConfigurationBuilder();

        var action = () => builder.AddRaycynixConfigurationSources(options => options.BaseFileName = string.Empty);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*Base configuration file name cannot be null or whitespace.*");
    }
}

using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace Raycynix.Extensions.Configuration.Tests.Sources;

/// <summary>
/// Covers standard configuration source registration and ordering.
/// </summary>
public class ConfigurationSourcesTests
{
    /// <summary>
    /// Verifies that the standard source order applies base JSON, environment JSON and command-line arguments in the expected priority.
    /// </summary>
    [Fact]
    public void UseRaycynixConfigurationSources_ShouldApplyStandardSourcePriority()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"raycynix-config-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);

        try
        {
            File.WriteAllText(
                Path.Combine(tempDirectory, "appsettings.json"),
                """
                {
                  "SourceOptions": {
                    "Value": "from-base",
                    "OnlyBase": "base-only"
                  }
                }
                """);

            File.WriteAllText(
                Path.Combine(tempDirectory, "appsettings.Testing.json"),
                """
                {
                  "SourceOptions": {
                    "Value": "from-environment",
                    "OnlyEnvironment": "environment-only"
                  }
                }
                """);

            var configuration = new ConfigurationBuilder()
                .UseRaycynixConfigurationSources(options =>
                {
                    options.BasePath = tempDirectory;
                    options.BaseFileName = "appsettings";
                    options.EnvironmentName = EnvironmentNames.Testing;
                    options.BaseJsonOptional = false;
                    options.ReloadOnChange = false;
                    options.CommandLineArguments =
                    [
                        "SourceOptions:Value=from-command-line"
                    ];
                })
                .Build();

            configuration["SourceOptions:Value"].Should().Be("from-command-line");
            configuration["SourceOptions:OnlyBase"].Should().Be("base-only");
            configuration["SourceOptions:OnlyEnvironment"].Should().Be("environment-only");
        }
        finally
        {
            Directory.Delete(tempDirectory, true);
        }
    }
}

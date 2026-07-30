using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace Raycynix.Extensions.Configuration.Tests.Sources;

/// <summary>
/// Covers standard configuration source registration and ordering.
/// </summary>
public class ConfigurationSourcesTests
{
    /// <summary>
    /// Verifies that the standard source order applies JSON, dotenv and command-line values in the expected priority.
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

            File.WriteAllText(
                Path.Combine(tempDirectory, ".env"),
                """
                SourceOptions__Value=from-dotenv
                SourceOptions__OnlyEnvFile=dotenv-only
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
            configuration["SourceOptions:OnlyEnvFile"].Should().Be("dotenv-only");
        }
        finally
        {
            Directory.Delete(tempDirectory, true);
        }
    }

    /// <summary>
    /// Verifies that process environment variables override values loaded from the dotenv file.
    /// </summary>
    [Fact]
    public void UseRaycynixConfigurationSources_ShouldPreferProcessEnvironmentOverDotEnv()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"raycynix-config-tests-{Guid.NewGuid():N}");
        var keyPrefix = $"RAYCYNIX_TEST_{Guid.NewGuid():N}";
        var environmentKey = $"{keyPrefix}__Value";
        Directory.CreateDirectory(tempDirectory);

        try
        {
            File.WriteAllText(
                Path.Combine(tempDirectory, ".env"),
                $"{keyPrefix}__Value=from-dotenv");
            System.Environment.SetEnvironmentVariable(environmentKey, "from-process-environment");

            var configuration = new ConfigurationBuilder()
                .UseRaycynixConfigurationSources(options =>
                {
                    options.BasePath = tempDirectory;
                    options.ReloadOnChange = false;
                })
                .Build();

            configuration[$"{keyPrefix}:Value"].Should().Be("from-process-environment");
        }
        finally
        {
            System.Environment.SetEnvironmentVariable(environmentKey, null);
            Directory.Delete(tempDirectory, true);
        }
    }
}

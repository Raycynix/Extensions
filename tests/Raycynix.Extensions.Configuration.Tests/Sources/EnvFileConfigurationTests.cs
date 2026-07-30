using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace Raycynix.Extensions.Configuration.Tests.Sources;

/// <summary>
/// Covers dotenv file parsing and registration behavior.
/// </summary>
public class EnvFileConfigurationTests
{
    /// <summary>
    /// Verifies the common dotenv syntax supported by the configuration provider.
    /// </summary>
    [Fact]
    public void AddEnvFile_ShouldParseCommonDotEnvSyntax()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"raycynix-env-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);

        try
        {
            File.WriteAllText(
                Path.Combine(tempDirectory, ".env"),
                """
                # Comment
                PLAIN=value
                EMPTY=
                export NESTED__VALUE=from-nested
                DOUBLE_QUOTED="value with spaces # preserved"
                SINGLE_QUOTED='literal \n value'
                INLINE_COMMENT=value # ignored
                URL=https://example.test/#fragment
                ESCAPED="first\nsecond"
                WINDOWS_PATH="C:\work\app"
                """);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(tempDirectory)
                .AddEnvFile()
                .Build();

            configuration["PLAIN"].Should().Be("value");
            configuration["EMPTY"].Should().BeEmpty();
            configuration["NESTED:VALUE"].Should().Be("from-nested");
            configuration["DOUBLE_QUOTED"].Should().Be("value with spaces # preserved");
            configuration["SINGLE_QUOTED"].Should().Be(@"literal \n value");
            configuration["INLINE_COMMENT"].Should().Be("value");
            configuration["URL"].Should().Be("https://example.test/#fragment");
            configuration["ESCAPED"].Should().Be("first\nsecond");
            configuration["WINDOWS_PATH"].Should().Be(@"C:\work\app");
        }
        finally
        {
            Directory.Delete(tempDirectory, true);
        }
    }

    /// <summary>
    /// Verifies that an explicitly required dotenv file must exist.
    /// </summary>
    [Fact]
    public void AddEnvFile_ShouldThrow_WhenRequiredFileIsMissing()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"raycynix-env-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);

        try
        {
            var action = () => new ConfigurationBuilder()
                .SetBasePath(tempDirectory)
                .AddEnvFile(optional: false)
                .Build();

            action.Should().Throw<FileNotFoundException>();
        }
        finally
        {
            Directory.Delete(tempDirectory, true);
        }
    }
}

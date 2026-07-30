using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;

namespace Raycynix.Extensions.Configuration.AspNetCore.Tests.Builder;

/// <summary>
/// Covers ASP.NET Core builder integration for the Raycynix configuration package.
/// </summary>
public class AspNetCoreConfigurationBuilderTests
{
    /// <summary>
    /// Verifies that ASP.NET Core configuration registration uses the content root for standard JSON and dotenv sources.
    /// </summary>
    [Fact]
    public void AddRaycynixAspNetCoreConfiguration_ShouldRegisterApplicationEnvironmentAndStandardSources()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"raycynix-aspnet-config-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);

        try
        {
            File.WriteAllText(
                Path.Combine(tempDirectory, "appsettings.json"),
                """
                {
                  "SampleOptions": {
                    "Value": "from-base"
                  }
                }
                """);

            File.WriteAllText(
                Path.Combine(tempDirectory, "appsettings.Development.json"),
                """
                {
                  "SampleOptions": {
                    "Value": "from-environment"
                  }
                }
                """);

            File.WriteAllText(
                Path.Combine(tempDirectory, ".env"),
                "SampleOptions__EnvValue=from-dotenv");

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = EnvironmentNames.Development,
                ContentRootPath = tempDirectory
            });

            builder.AddRaycynixAspNetCoreConfiguration(options =>
            {
                options.BaseFileName = "appsettings";
                options.ReloadOnChange = false;
                options.IncludeUserSecrets = false;
                options.BaseJsonOptional = false;
            });

            builder.Services.AddRaycynixConfiguration<SampleOptions>(builder.Configuration);

            using var app = builder.Build();
            var environment = app.Services.GetRequiredService<IApplicationEnvironment>();
            var sampleOptions = app.Services.GetRequiredService<IConfigurationAccessor<SampleOptions>>();

            environment.Name.Should().Be(EnvironmentNames.Development);
            environment.IsDevelopment.Should().BeTrue();
            sampleOptions.Current.Value.Should().Be("from-environment");
            sampleOptions.Current.EnvValue.Should().Be("from-dotenv");
        }
        finally
        {
            Directory.Delete(tempDirectory, true);
        }
    }

    private class SampleOptions
    {
        public string Value { get; set; } = string.Empty;

        public string EnvValue { get; set; } = string.Empty;
    }
}

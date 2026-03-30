using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;

namespace Raycynix.Extensions.Configuration.Tests.Environment;

/// <summary>
/// Covers application environment registration and helpers.
/// </summary>
public class ApplicationEnvironmentTests
{
    /// <summary>
    /// Verifies that the explicit environment registration exposes the expected environment flags.
    /// </summary>
    [Fact]
    public void AddRaycynixEnvironment_ShouldRegisterExplicitEnvironment()
    {
        var services = new ServiceCollection();
        services.AddRaycynixEnvironment(EnvironmentNames.Testing);

        using var provider = services.BuildServiceProvider();
        var environment = provider.GetRequiredService<IApplicationEnvironment>();

        environment.Name.Should().Be(EnvironmentNames.Testing);
        environment.IsTesting.Should().BeTrue();
        environment.IsDevelopment.Should().BeFalse();
        environment.IsProduction.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that the host-based environment registration reads the environment name from the host environment.
    /// </summary>
    [Fact]
    public void AddRaycynixEnvironment_ShouldUseHostEnvironmentWhenNoExplicitNameIsProvided()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(new TestHostEnvironment { EnvironmentName = EnvironmentNames.Staging });
        services.AddRaycynixEnvironment();

        using var provider = services.BuildServiceProvider();
        var environment = provider.GetRequiredService<IApplicationEnvironment>();

        environment.Name.Should().Be(EnvironmentNames.Staging);
        environment.IsStaging.Should().BeTrue();
    }

    private class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = string.Empty;

        public string ApplicationName { get; set; } = "Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } =
            new PhysicalFileProvider(AppContext.BaseDirectory);
    }
}

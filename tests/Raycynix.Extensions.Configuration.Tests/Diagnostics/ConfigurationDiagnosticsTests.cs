using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Configuration.Abstractions.Enums;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;

namespace Raycynix.Extensions.Configuration.Tests.Diagnostics;

/// <summary>
/// Covers configuration diagnostics, retained reload history, and redacted snapshots.
/// </summary>
public class ConfigurationDiagnosticsTests
{
    /// <summary>
    /// Verifies that diagnostics expose registered typed options metadata.
    /// </summary>
    [Fact]
    public async Task GetRegistrations_ShouldReturnRegisteredOptionsMetadata()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DiagnosticsOptions:Name"] = "primary"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRaycynixConfiguration<DiagnosticsOptions>(
            configuration,
            requireSection: true);

        using var provider = services.BuildServiceProvider();
        await StartHostedServicesAsync(provider, TestContext.Current.CancellationToken);

        try
        {
            var diagnostics = provider.GetRequiredService<IConfigurationDiagnostics>();

            diagnostics.GetRegistrations()
                .Should()
                .ContainSingle(registration =>
                    registration.OptionsType == typeof(DiagnosticsOptions) &&
                    registration.SectionName == "DiagnosticsOptions" &&
                    registration.OptionsName == Microsoft.Extensions.Options.Options.DefaultName &&
                    registration.RequiredSection);
        }
        finally
        {
            await StopHostedServicesAsync(provider, TestContext.Current.CancellationToken);
        }
    }

    /// <summary>
    /// Verifies that diagnostics return a recursively redacted approved configuration snapshot.
    /// </summary>
    [Fact]
    public void GetRedactedSnapshot_ShouldRedactNestedObjectsDictionariesAndCollections()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SecretOptions:Name"] = "visible",
                ["SecretOptions:Password"] = "pass",
                ["SecretOptions:Nested:ApiKey"] = "api-key",
                ["SecretOptions:Values:ConnectionString"] = "Host=primary;",
                ["SecretOptions:Items:0:Token"] = "token",
                ["SecretOptions:Items:0:Name"] = "item"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddRaycynixConfiguration<SecretOptions>(configuration);

        using var provider = services.BuildServiceProvider();
        var diagnostics = provider.GetRequiredService<IConfigurationDiagnostics>();

        var snapshot = diagnostics.GetRedactedSnapshot<SecretOptions>()
            .Should()
            .BeAssignableTo<Dictionary<string, object?>>()
            .Subject;

        snapshot["Name"].Should().Be("visible");
        snapshot["Password"].Should().Be("***");

        var nested = snapshot["Nested"].Should().BeAssignableTo<Dictionary<string, object?>>().Subject;
        nested["ApiKey"].Should().Be("***");

        var values = snapshot["Values"].Should().BeAssignableTo<Dictionary<string, object?>>().Subject;
        values["ConnectionString"].Should().Be("***");

        var items = snapshot["Items"].Should().BeAssignableTo<List<object?>>().Subject;
        var item = items[0].Should().BeAssignableTo<Dictionary<string, object?>>().Subject;
        item["Token"].Should().Be("***");
        item["Name"].Should().Be("item");
    }

    /// <summary>
    /// Verifies that redacted snapshots can be disabled through diagnostics options.
    /// </summary>
    [Fact]
    public void GetRedactedSnapshot_ShouldThrow_WhenSnapshotsAreDisabled()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DiagnosticsOptions:Name"] = "primary"
            })
            .Build();

        var services = new ServiceCollection();
        services.ConfigureRaycynixConfigurationDiagnostics(options => options.EnableSnapshots = false);
        services.AddRaycynixConfiguration<DiagnosticsOptions>(configuration);

        using var provider = services.BuildServiceProvider();
        var diagnostics = provider.GetRequiredService<IConfigurationDiagnostics>();

        var action = () => diagnostics.GetRedactedSnapshot<DiagnosticsOptions>();

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Configuration snapshots are disabled.");
    }

    /// <summary>
    /// Verifies that applications can replace the default redactor with an inline delegate.
    /// </summary>
    [Fact]
    public void AddRaycynixConfigurationRedactor_ShouldUseInlineRedactor()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LicenseOptions:License"] = "secret-license",
                ["LicenseOptions:Name"] = "visible"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddRaycynixConfigurationRedactor((key, value) =>
            key.Contains("License", StringComparison.OrdinalIgnoreCase) ? "***" : value);
        services.AddRaycynixConfiguration<LicenseOptions>(configuration);

        using var provider = services.BuildServiceProvider();
        var diagnostics = provider.GetRequiredService<IConfigurationDiagnostics>();

        var snapshot = diagnostics.GetRedactedSnapshot<LicenseOptions>()
            .Should()
            .BeAssignableTo<Dictionary<string, object?>>()
            .Subject;

        snapshot["License"].Should().Be("***");
        snapshot["Name"].Should().Be("visible");
    }

    /// <summary>
    /// Verifies that applications can replace the default redactor with a custom redactor type.
    /// </summary>
    [Fact]
    public void AddRaycynixConfigurationRedactor_ShouldUseCustomRedactorType()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["LicenseOptions:License"] = "secret-license",
                ["LicenseOptions:Name"] = "visible"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddRaycynixConfigurationRedactor<LicenseConfigurationRedactor>();
        services.AddRaycynixConfiguration<LicenseOptions>(configuration);

        using var provider = services.BuildServiceProvider();
        var diagnostics = provider.GetRequiredService<IConfigurationDiagnostics>();

        var snapshot = diagnostics.GetRedactedSnapshot<LicenseOptions>()
            .Should()
            .BeAssignableTo<Dictionary<string, object?>>()
            .Subject;

        snapshot["License"].Should().Be("[redacted-license]");
        snapshot["Name"].Should().Be("visible");
    }


    /// <summary>
    /// Verifies that retained reload history respects the configured per-options limit.
    /// </summary>
    [Fact]
    public async Task GetReloads_ShouldRespectConfiguredHistoryLimit()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DiagnosticsOptions:Name"] = "initial"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.ConfigureRaycynixConfigurationDiagnostics(options => options.MaxReloadHistoryPerOptions = 2);
        services.AddRaycynixConfiguration<DiagnosticsOptions>(configuration);

        using var provider = services.BuildServiceProvider();
        await StartHostedServicesAsync(provider, TestContext.Current.CancellationToken);

        try
        {
            configuration["DiagnosticsOptions:Name"] = "first";
            configuration.Reload();
            await Task.Delay(100, TestContext.Current.CancellationToken);

            configuration["DiagnosticsOptions:Name"] = "second";
            configuration.Reload();
            await Task.Delay(100, TestContext.Current.CancellationToken);

            configuration["DiagnosticsOptions:Name"] = "third";
            configuration.Reload();
            await Task.Delay(100, TestContext.Current.CancellationToken);

            var diagnostics = provider.GetRequiredService<IConfigurationDiagnostics>();

            diagnostics.GetReloads()
                .Where(reload => reload.OptionsType == typeof(DiagnosticsOptions))
                .Should()
                .HaveCount(2)
                .And
                .OnlyContain(reload => reload.Behavior == ConfigurationReloadBehavior.Apply);
        }
        finally
        {
            await StopHostedServicesAsync(provider, TestContext.Current.CancellationToken);
        }
    }

    private static async Task StartHostedServicesAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        foreach (var hostedService in provider.GetServices<IHostedService>())
        {
            await hostedService.StartAsync(cancellationToken);
        }
    }

    private static async Task StopHostedServicesAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        foreach (var hostedService in provider.GetServices<IHostedService>())
        {
            await hostedService.StopAsync(cancellationToken);
        }
    }

    private sealed class DiagnosticsOptions
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class SecretOptions
    {
        public string Name { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public NestedSecretOptions Nested { get; set; } = new();

        public Dictionary<string, string> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public List<SecretItemOptions> Items { get; set; } = [];
    }

    private sealed class NestedSecretOptions
    {
        public string ApiKey { get; set; } = string.Empty;
    }

    private sealed class SecretItemOptions
    {
        public string Token { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }

    private sealed class LicenseOptions
    {
        public string License { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }

    private sealed class LicenseConfigurationRedactor : IConfigurationRedactor
    {
        public object? Redact(string key, object? value)
        {
            return key.Contains("License", StringComparison.OrdinalIgnoreCase)
                ? "[redacted-license]"
                : value;
        }
    }
}

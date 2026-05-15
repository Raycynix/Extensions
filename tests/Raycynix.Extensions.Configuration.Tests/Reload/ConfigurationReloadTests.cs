using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Configuration.Abstractions.Attributes;
using Raycynix.Extensions.Configuration.Abstractions.Enums;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;

namespace Raycynix.Extensions.Configuration.Tests.Reload;

/// <summary>
/// Covers runtime reload behavior and approved snapshot handling.
/// </summary>
public class ConfigurationReloadTests
{
    /// <summary>
    /// Verifies that an explicit reload policy can reject a runtime change and keep the last approved snapshot.
    /// </summary>
    [Fact]
    public async Task AddRaycynixConfiguration_ShouldKeepLastApprovedSnapshotWhenReloadIsRejectedByPolicy()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CacheOptions:ConnectionString"] = "Host=primary;",
                ["CacheOptions:DefaultTtlSeconds"] = "30"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRaycynixConfiguration<CacheOptions>(configuration);
        services.AddRaycynixConfigurationReloadPolicy<CacheOptions>(context =>
        {
            return context.Previous.ConnectionString != context.Current.ConnectionString
                ? ConfigurationReloadResult.Reject("Connection string changes are not allowed at runtime.")
                : ConfigurationReloadResult.Apply();
        });

        using var provider = services.BuildServiceProvider();
        await StartHostedServicesAsync(provider, TestContext.Current.CancellationToken);

        try
        {
            var accessor = provider.GetRequiredService<IConfigurationAccessor<CacheOptions>>();
            accessor.Current.ConnectionString.Should().Be("Host=primary;");

            configuration["CacheOptions:ConnectionString"] = "Host=secondary;";
            configuration.Reload();

            await Task.Delay(100, TestContext.Current.CancellationToken);

            accessor.Current.ConnectionString.Should().Be("Host=primary;");
            accessor.Current.DefaultTtlSeconds.Should().Be(30);
        }
        finally
        {
            await StopHostedServicesAsync(provider, TestContext.Current.CancellationToken);
        }
    }

    /// <summary>
    /// Verifies that attribute-based reload rules also reject runtime changes and preserve the approved snapshot.
    /// </summary>
    [Fact]
    public async Task AddRaycynixConfiguration_ShouldKeepLastApprovedSnapshotWhenReloadIsRejectedByAttribute()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AttributeProtectedOptions:ConnectionString"] = "Host=primary;",
                ["AttributeProtectedOptions:DefaultTtlSeconds"] = "30"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRaycynixConfiguration<AttributeProtectedOptions>(configuration);

        using var provider = services.BuildServiceProvider();
        await StartHostedServicesAsync(provider, TestContext.Current.CancellationToken);

        try
        {
            var accessor = provider.GetRequiredService<IConfigurationAccessor<AttributeProtectedOptions>>();
            accessor.Current.ConnectionString.Should().Be("Host=primary;");

            configuration["AttributeProtectedOptions:ConnectionString"] = "Host=secondary;";
            configuration.Reload();

            await Task.Delay(100, TestContext.Current.CancellationToken);

            accessor.Current.ConnectionString.Should().Be("Host=primary;");
            accessor.Current.DefaultTtlSeconds.Should().Be(30);
        }
        finally
        {
            await StopHostedServicesAsync(provider, TestContext.Current.CancellationToken);
        }
    }

    /// <summary>
    /// Verifies that ignored runtime changes keep the approved snapshot and are recorded in diagnostics.
    /// </summary>
    [Fact]
    public async Task AddRaycynixConfiguration_ShouldKeepLastApprovedSnapshotWhenReloadIsIgnored()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CacheOptions:ConnectionString"] = "Host=primary;",
                ["CacheOptions:DefaultTtlSeconds"] = "30"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRaycynixConfiguration<CacheOptions>(configuration);
        services.AddRaycynixConfigurationReloadPolicy<CacheOptions>(_ =>
            ConfigurationReloadResult.Ignore("The change is intentionally ignored."));

        using var provider = services.BuildServiceProvider();
        await StartHostedServicesAsync(provider, TestContext.Current.CancellationToken);

        try
        {
            var accessor = provider.GetRequiredService<IConfigurationAccessor<CacheOptions>>();
            var diagnostics = provider.GetRequiredService<IConfigurationDiagnostics>();

            configuration["CacheOptions:DefaultTtlSeconds"] = "60";
            configuration.Reload();

            await Task.Delay(100, TestContext.Current.CancellationToken);

            accessor.Current.DefaultTtlSeconds.Should().Be(30);
            diagnostics.GetReloads()
                .Should()
                .ContainSingle(reload =>
                    reload.OptionsType == typeof(CacheOptions) &&
                    reload.Behavior == ConfigurationReloadBehavior.Ignore &&
                    reload.Reason == "The change is intentionally ignored.");
        }
        finally
        {
            await StopHostedServicesAsync(provider, TestContext.Current.CancellationToken);
        }
    }

    /// <summary>
    /// Verifies that restart-required runtime changes keep the approved snapshot and are recorded in diagnostics.
    /// </summary>
    [Fact]
    public async Task AddRaycynixConfiguration_ShouldKeepLastApprovedSnapshotWhenReloadRequiresRestart()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CacheOptions:ConnectionString"] = "Host=primary;",
                ["CacheOptions:DefaultTtlSeconds"] = "30"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRaycynixConfiguration<CacheOptions>(configuration);
        services.AddRaycynixConfigurationReloadPolicy<CacheOptions>(_ =>
            ConfigurationReloadResult.RestartRequired("Restart is required."));

        using var provider = services.BuildServiceProvider();
        await StartHostedServicesAsync(provider, TestContext.Current.CancellationToken);

        try
        {
            var accessor = provider.GetRequiredService<IConfigurationAccessor<CacheOptions>>();
            var diagnostics = provider.GetRequiredService<IConfigurationDiagnostics>();

            configuration["CacheOptions:DefaultTtlSeconds"] = "60";
            configuration.Reload();

            await Task.Delay(100, TestContext.Current.CancellationToken);

            accessor.Current.DefaultTtlSeconds.Should().Be(30);
            diagnostics.GetReloads()
                .Should()
                .ContainSingle(reload =>
                    reload.OptionsType == typeof(CacheOptions) &&
                    reload.Behavior == ConfigurationReloadBehavior.RestartRequired &&
                    reload.Reason == "Restart is required.");
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

    private class CacheOptions
    {
        public string ConnectionString { get; set; } = string.Empty;

        public int DefaultTtlSeconds { get; set; }
    }

    private class AttributeProtectedOptions
    {
        [ConfigurationReloadBehavior(ConfigurationReloadBehavior.Reject)]
        public string ConnectionString { get; set; } = string.Empty;

        public int DefaultTtlSeconds { get; set; }
    }
}

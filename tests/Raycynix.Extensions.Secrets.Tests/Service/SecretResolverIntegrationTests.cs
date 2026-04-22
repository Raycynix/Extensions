using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Secrets.Extensions;
using Raycynix.Extensions.Secrets.Implementations;
using Raycynix.Extensions.Security.Abstractions.Interfaces;

namespace Raycynix.Extensions.Secrets.Tests.Service;

/// <summary>
/// Covers the default secrets provider chain registered through DI.
/// </summary>
public sealed class SecretResolverIntegrationTests
{
    /// <summary>
    /// Verifies that configuration values are preferred over fallback environment providers.
    /// </summary>
    [Fact]
    public async Task GetSecretAsync_ShouldPreferConfigurationValue_WhenMultipleProvidersHaveAValue()
    {
        const string key = "ConnectionStrings:Main";
        const string configurationValue = "Server=config;Database=main;";
        const string environmentValue = "Server=env;Database=main;";

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [key] = configurationValue
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddRaycynixSecrets();

        Environment.SetEnvironmentVariable(key, environmentValue);

        try
        {
            await using var serviceProvider = services.BuildServiceProvider();
            var resolver = serviceProvider.GetRequiredService<ISecretResolver>();

            var result = await resolver.GetSecretAsync(key, TestContext.Current.CancellationToken);

            result.Should().Be(configurationValue);
        }
        finally
        {
            Environment.SetEnvironmentVariable(key, null);
        }
    }

    /// <summary>
    /// Verifies that the configured provider order changes the resolved value.
    /// </summary>
    [Fact]
    public async Task GetSecretAsync_ShouldHonorConfiguredProviderOrder_FromServiceRegistration()
    {
        const string key = "ConnectionStrings:Main";
        const string configurationValue = "Server=config;Database=main;";
        const string gitHubValue = "Server=github;Database=main;";

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [key] = configurationValue
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddRaycynixSecrets(options =>
        {
            options.ProviderOrder.Clear();
            options.ProviderOrder.Add(typeof(GitHubSecretProvider));
            options.ProviderOrder.Add(typeof(ConfigurationSecretProvider));
        });

        Environment.SetEnvironmentVariable("CONNECTIONSTRINGS_MAIN", gitHubValue);

        try
        {
            await using var serviceProvider = services.BuildServiceProvider();
            var resolver = serviceProvider.GetRequiredService<ISecretResolver>();

            var result = await resolver.GetSecretAsync(key, TestContext.Current.CancellationToken);

            result.Should().Be(gitHubValue);
        }
        finally
        {
            Environment.SetEnvironmentVariable("CONNECTIONSTRINGS_MAIN", null);
        }
    }

    /// <summary>
    /// Verifies that provider-aware resolution returns the winning provider name.
    /// </summary>
    [Fact]
    public async Task ResolveWithSourceAsync_ShouldReturnWinningProviderName()
    {
        const string key = "ConnectionStrings:Main";
        const string configurationValue = "Server=config;Database=main;";

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [key] = configurationValue
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddRaycynixSecrets();

        await using var serviceProvider = services.BuildServiceProvider();
        var resolver = serviceProvider.GetRequiredService<ISecretResolver>();

        var result = await resolver.ResolveWithSourceAsync(key, TestContext.Current.CancellationToken);

        result.Succeeded.Should().BeTrue();
        result.ProviderName.Should().Be(nameof(ConfigurationSecretProvider));
    }

    /// <summary>
    /// Verifies that explain output includes the providers evaluated by the default chain.
    /// </summary>
    [Fact]
    public async Task ExplainSecretResolutionAsync_ShouldReturnProviderAttempts_FromServiceRegistration()
    {
        const string key = "ConnectionStrings:Main";
        const string gitHubValue = "Server=github;Database=main;";

        var services = new ServiceCollection();
        services.AddRaycynixSecrets();

        Environment.SetEnvironmentVariable("CONNECTIONSTRINGS_MAIN", gitHubValue);

        try
        {
            await using var serviceProvider = services.BuildServiceProvider();
            var resolver = serviceProvider.GetRequiredService<ISecretResolver>();

            var attempts = await resolver.ExplainSecretResolutionAsync(key, TestContext.Current.CancellationToken);

            attempts.Should().Contain(attempt => attempt.ProviderName == nameof(ConfigurationSecretProvider));
            attempts.Should().Contain(attempt => attempt.ProviderName == nameof(EnvironmentSecretProvider));
            attempts.Should().Contain(attempt => attempt.ProviderName == nameof(GitHubSecretProvider) && attempt.Succeeded);
        }
        finally
        {
            Environment.SetEnvironmentVariable("CONNECTIONSTRINGS_MAIN", null);
        }
    }
}

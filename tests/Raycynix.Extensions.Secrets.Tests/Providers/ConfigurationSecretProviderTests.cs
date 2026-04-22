using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Secrets.Implementations;

namespace Raycynix.Extensions.Secrets.Tests.Providers;

/// <summary>
/// Covers configuration-backed secret resolution.
/// </summary>
public sealed class ConfigurationSecretProviderTests
{
    /// <summary>
    /// Verifies that the provider resolves values from the application configuration pipeline.
    /// </summary>
    [Fact]
    public async Task GetSecretAsync_ShouldReturnMatchingConfigurationValue()
    {
        const string key = "Secrets:ApiToken";
        const string value = "config-secret";

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [key] = value
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        using var serviceProvider = services.BuildServiceProvider();

        var provider = new ConfigurationSecretProvider(serviceProvider);

        var result = await provider.GetSecretAsync(key, TestContext.Current.CancellationToken);

        result.Should().Be(value);
    }
}

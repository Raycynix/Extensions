using FluentAssertions;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Secrets.Implementations;
using Raycynix.Extensions.Security.Abstractions.Interfaces;

namespace Raycynix.Extensions.Secrets.Tests.Service;

/// <summary>
/// Covers secret resolution behavior through the composite resolver.
/// </summary>
public sealed class CompositeSecretResolverTests
{
    /// <summary>
    /// Verifies that the resolver returns the first non-empty secret from the provider chain.
    /// </summary>
    [Fact]
    public async Task GetSecretAsync_ShouldReturnFirstNonEmptySecret()
    {
        var resolver = new CompositeSecretResolver(
        [
            new StaticSecretProvider(null),
            new StaticSecretProvider(""),
            new StaticSecretProvider("resolved-secret")
        ]);

        var result = await resolver.GetSecretAsync("GitHub:Token", TestContext.Current.CancellationToken);

        result.Should().Be("resolved-secret");
    }

    /// <summary>
    /// Verifies that the resolver returns null when no provider has a value.
    /// </summary>
    [Fact]
    public async Task GetSecretAsync_ShouldReturnNull_WhenNoProviderResolvesValue()
    {
        var resolver = new CompositeSecretResolver(
        [
            new StaticSecretProvider(null),
            new StaticSecretProvider(" ")
        ]);

        var result = await resolver.GetSecretAsync("GitHub:Token", TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that the resolver respects the configured provider order.
    /// </summary>
    [Fact]
    public async Task GetSecretAsync_ShouldHonorConfiguredProviderOrder()
    {
        var resolver = new CompositeSecretResolver(
        [
            new TestEnvironmentProvider(),
            new TestConfigurationProvider()
        ],
        Options.Create(new SecretOptions
        {
            ProviderOrder =
            {
                typeof(TestConfigurationProvider),
                typeof(TestEnvironmentProvider)
            }
        }));

        var result = await resolver.GetSecretAsync("Api:Token", TestContext.Current.CancellationToken);

        result.Should().Be("config-secret");
    }

    /// <summary>
    /// Verifies that the resolver reports the provider that resolved the secret.
    /// </summary>
    [Fact]
    public async Task ResolveSecretAsync_ShouldReturnProviderName()
    {
        var resolver = new CompositeSecretResolver(
        [
            new StaticSecretProvider(null),
            new TestConfigurationProvider()
        ]);

        var result = await resolver.ResolveSecretAsync("Api:Token", TestContext.Current.CancellationToken);

        result.Succeeded.Should().BeTrue();
        result.Value.Should().Be("config-secret");
        result.ProviderName.Should().Be(nameof(TestConfigurationProvider));
    }

    /// <summary>
    /// Verifies that explain output reports the evaluated provider chain until resolution succeeds.
    /// </summary>
    [Fact]
    public async Task ExplainSecretResolutionAsync_ShouldReturnProviderAttempts()
    {
        var resolver = new CompositeSecretResolver(
        [
            new StaticSecretProvider(null),
            new TestConfigurationProvider(),
            new TestEnvironmentProvider()
        ]);

        var attempts = await resolver.ExplainSecretResolutionAsync("Api:Token", TestContext.Current.CancellationToken);

        attempts.Should().HaveCount(2);
        attempts[0].ProviderName.Should().Be(nameof(StaticSecretProvider));
        attempts[0].Succeeded.Should().BeFalse();
        attempts[1].ProviderName.Should().Be(nameof(TestConfigurationProvider));
        attempts[1].Succeeded.Should().BeTrue();
    }

    private sealed class StaticSecretProvider(string? value) : ISecretProvider
    {
        public ValueTask<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(value);
        }
    }

    private sealed class TestConfigurationProvider : ISecretProvider
    {
        public ValueTask<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult<string?>("config-secret");
        }
    }

    private sealed class TestEnvironmentProvider : ISecretProvider
    {
        public ValueTask<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult<string?>("env-secret");
        }
    }
}

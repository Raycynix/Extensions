using FluentAssertions;
using Raycynix.Extensions.Secrets.Implementations;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using SecretOptions = Raycynix.Extensions.Secrets.Options.SecretOptions;

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
        new SecretOptions
        {
            ProviderOrder =
            [
                nameof(TestConfigurationProvider),
                nameof(TestEnvironmentProvider)
            ]
        });

        var result = await resolver.GetSecretAsync("Api:Token", TestContext.Current.CancellationToken);

        result.Should().Be("config-secret");
    }

    /// <summary>
    /// Verifies that an unknown configured provider is rejected instead of being silently ignored.
    /// </summary>
    [Fact]
    public void Constructor_ShouldRejectUnknownConfiguredProvider()
    {
        var action = () => new CompositeSecretResolver(
        [
            new TestConfigurationProvider()
        ],
        new SecretOptions
        {
            ProviderOrder = ["MissingProvider"]
        });

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*not registered*MissingProvider*");
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

    [Fact]
    public async Task GetSecretAsync_ShouldContinueAfterProviderFailure_ByDefault()
    {
        var resolver = new CompositeSecretResolver(
        [
            new ThrowingSecretProvider(new InvalidOperationException("provider unavailable")),
            new TestConfigurationProvider()
        ]);

        var result = await resolver.GetSecretAsync("Api:Token", TestContext.Current.CancellationToken);

        result.Should().Be("config-secret");
    }

    [Fact]
    public async Task GetSecretAsync_ShouldFailFast_WhenConfigured()
    {
        var failure = new InvalidOperationException("provider unavailable");
        var resolver = new CompositeSecretResolver(
        [
            new ThrowingSecretProvider(failure),
            new TestConfigurationProvider()
        ],
        new SecretOptions { ContinueOnProviderError = false });

        var act = async () => await resolver.GetSecretAsync("Api:Token", TestContext.Current.CancellationToken);

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Should().BeSameAs(failure);
    }

    [Fact]
    public async Task GetSecretAsync_ShouldNotSwallowRequestedCancellation()
    {
        using var cancellationSource = new CancellationTokenSource();
        await cancellationSource.CancelAsync();
        var resolver = new CompositeSecretResolver(
        [
            new ThrowingSecretProvider(new OperationCanceledException(cancellationSource.Token)),
            new TestConfigurationProvider()
        ]);

        var act = async () => await resolver.GetSecretAsync("Api:Token", cancellationSource.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
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

    private sealed class ThrowingSecretProvider(Exception exception) : ISecretProvider
    {
        public ValueTask<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromException<string?>(exception);
        }
    }
}

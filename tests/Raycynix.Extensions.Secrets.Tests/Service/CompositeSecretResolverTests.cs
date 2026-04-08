using FluentAssertions;
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

    private sealed class StaticSecretProvider(string? value) : ISecretProvider
    {
        public ValueTask<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(value);
        }
    }
}

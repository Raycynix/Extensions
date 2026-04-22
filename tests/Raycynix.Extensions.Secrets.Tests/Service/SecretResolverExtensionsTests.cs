using FluentAssertions;
using Raycynix.Extensions.Secrets.Exceptions;
using Raycynix.Extensions.Secrets.Extensions;
using Raycynix.Extensions.Security.Abstractions.Interfaces;

namespace Raycynix.Extensions.Secrets.Tests.Service;

/// <summary>
/// Covers extension-based convenience APIs for secret resolution.
/// </summary>
public sealed class SecretResolverExtensionsTests
{
    /// <summary>
    /// Verifies that a required secret is returned when available.
    /// </summary>
    [Fact]
    public async Task GetRequiredSecretAsync_ShouldReturnValue_WhenSecretExists()
    {
        ISecretResolver resolver = new StaticResolver("resolved-secret");

        var result = await resolver.GetRequiredSecretAsync("Api:Token", TestContext.Current.CancellationToken);

        result.Should().Be("resolved-secret");
    }

    /// <summary>
    /// Verifies that a required secret throws a dedicated exception when missing.
    /// </summary>
    [Fact]
    public async Task GetRequiredSecretAsync_ShouldThrow_WhenSecretIsMissing()
    {
        ISecretResolver resolver = new StaticResolver(null);

        var action = async () => await resolver.GetRequiredSecretAsync("Api:Token", TestContext.Current.CancellationToken);

        var exception = await action.Should().ThrowAsync<SecretNotFoundException>();
        exception.Which.Key.Should().Be("Api:Token");
    }

    private sealed class StaticResolver(string? value) : ISecretResolver
    {
        public ValueTask<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(value);
        }
    }
}

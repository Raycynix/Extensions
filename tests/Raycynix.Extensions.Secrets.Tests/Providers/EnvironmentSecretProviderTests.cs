using FluentAssertions;
using Raycynix.Extensions.Secrets.Implementations;

namespace Raycynix.Extensions.Secrets.Tests.Providers;

/// <summary>
/// Covers environment-variable secret resolution.
/// </summary>
public sealed class EnvironmentSecretProviderTests
{
    /// <summary>
    /// Verifies that the provider returns the value of the matching environment variable.
    /// </summary>
    [Fact]
    public async Task GetSecretAsync_ShouldReturnMatchingEnvironmentVariable()
    {
        const string key = "RAYCYNIX_SECRETS_ENVIRONMENT_TEST";
        const string value = "env-secret";
        Environment.SetEnvironmentVariable(key, value);

        try
        {
            var provider = new EnvironmentSecretProvider();

            var result = await provider.GetSecretAsync(key, TestContext.Current.CancellationToken);

            result.Should().Be(value);
        }
        finally
        {
            Environment.SetEnvironmentVariable(key, null);
        }
    }
}

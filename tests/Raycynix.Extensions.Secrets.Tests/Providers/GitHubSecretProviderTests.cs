using FluentAssertions;
using Raycynix.Extensions.Secrets.Implementations;

namespace Raycynix.Extensions.Secrets.Tests.Providers;

/// <summary>
/// Covers GitHub Actions-style secret resolution.
/// </summary>
public sealed class GitHubSecretProviderTests
{
    /// <summary>
    /// Verifies that the provider normalizes keys to GitHub Actions environment variable format.
    /// </summary>
    [Fact]
    public async Task GetSecretAsync_ShouldNormalizeKeyToGitHubEnvironmentFormat()
    {
        const string key = "GitHub:Token-Value.secret";
        const string normalizedKey = "GITHUB_TOKEN_VALUE_SECRET";
        const string value = "github-secret";
        Environment.SetEnvironmentVariable(normalizedKey, value);

        try
        {
            var provider = new GitHubSecretProvider();

            var result = await provider.GetSecretAsync(key, TestContext.Current.CancellationToken);

            result.Should().Be(value);
        }
        finally
        {
            Environment.SetEnvironmentVariable(normalizedKey, null);
        }
    }
}

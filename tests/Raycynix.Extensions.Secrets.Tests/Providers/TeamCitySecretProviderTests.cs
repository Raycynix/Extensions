using FluentAssertions;
using Raycynix.Extensions.Secrets.Implementations;

namespace Raycynix.Extensions.Secrets.Tests.Providers;

/// <summary>
/// Covers TeamCity-style secret resolution.
/// </summary>
public sealed class TeamCitySecretProviderTests
{
    /// <summary>
    /// Verifies that the provider maps colon-separated keys to TeamCity injected environment variables.
    /// </summary>
    [Fact]
    public async Task GetSecretAsync_ShouldNormalizeKeyToTeamCityEnvironmentFormat()
    {
        const string key = "TeamCity:Token";
        const string normalizedKey = "TeamCity.Token";
        const string value = "teamcity-secret";
        Environment.SetEnvironmentVariable(normalizedKey, value);

        try
        {
            var provider = new TeamCitySecretProvider();

            var result = await provider.GetSecretAsync(key, TestContext.Current.CancellationToken);

            result.Should().Be(value);
        }
        finally
        {
            Environment.SetEnvironmentVariable(normalizedKey, null);
        }
    }
}

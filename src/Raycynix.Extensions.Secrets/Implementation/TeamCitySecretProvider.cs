using Raycynix.Extensions.Security.Abstractions.Interfaces;

namespace Raycynix.Extensions.Secrets.Implementation;

/// <summary>
/// Resolves secrets from TeamCity-injected process variables.
/// </summary>
public sealed class TeamCitySecretProvider : ISecretProvider
{
    /// <inheritdoc />
    public ValueTask<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        var normalizedKey = key.Replace(':', '.');
        return ValueTask.FromResult(Environment.GetEnvironmentVariable($"env.{normalizedKey}"));
    }
}

using Raycynix.Extensions.Security.Abstractions.Interfaces;

namespace Raycynix.Extensions.Secrets.Implementations;

/// <summary>
/// Resolves secrets from GitHub Actions environment variables.
/// </summary>
public sealed class GitHubSecretProvider : ISecretProvider
{
    /// <inheritdoc />
    public ValueTask<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        var normalizedKey = key
            .Replace(':', '_')
            .Replace('.', '_')
            .Replace('-', '_')
            .ToUpperInvariant();

        return ValueTask.FromResult(Environment.GetEnvironmentVariable(normalizedKey));
    }
}

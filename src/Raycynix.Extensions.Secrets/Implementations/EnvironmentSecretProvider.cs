using Raycynix.Extensions.Security.Abstractions.Interfaces;

namespace Raycynix.Extensions.Secrets.Implementations;

/// <summary>
/// Resolves secrets from process environment variables.
/// </summary>
public sealed class EnvironmentSecretProvider : ISecretProvider
{
    /// <inheritdoc />
    public ValueTask<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(Environment.GetEnvironmentVariable(key));
    }
}

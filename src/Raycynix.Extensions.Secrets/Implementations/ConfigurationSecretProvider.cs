using Microsoft.Extensions.Configuration;
using Raycynix.Extensions.Security.Abstractions.Interfaces;

namespace Raycynix.Extensions.Secrets.Implementations;

/// <summary>
/// Resolves secrets from the application's configuration pipeline.
/// </summary>
public sealed class ConfigurationSecretProvider(IConfiguration configuration) : ISecretProvider
{
    /// <inheritdoc />
    public ValueTask<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        cancellationToken.ThrowIfCancellationRequested();

        return ValueTask.FromResult(configuration[key]);
    }
}

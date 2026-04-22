using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Security.Abstractions.Interfaces;

namespace Raycynix.Extensions.Secrets.Implementations;

/// <summary>
/// Resolves secrets from the application's configuration pipeline.
/// </summary>
public sealed class ConfigurationSecretProvider(IServiceProvider serviceProvider) : ISecretProvider
{
    /// <inheritdoc />
    public ValueTask<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        var configuration = serviceProvider.GetService<IConfiguration>();
        return ValueTask.FromResult(configuration?[key]);
    }
}

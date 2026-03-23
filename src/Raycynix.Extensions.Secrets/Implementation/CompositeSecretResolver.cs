using Raycynix.Extensions.Security.Abstractions.Interfaces;

namespace Raycynix.Extensions.Secrets.Implementation;

/// <summary>
/// Resolves secrets by querying registered providers in order and returning the first available value.
/// </summary>
public sealed class CompositeSecretResolver : ISecretResolver
{
    private readonly IReadOnlyCollection<ISecretProvider> _providers;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeSecretResolver"/> class.
    /// </summary>
    /// <param name="providers">The ordered list of secret providers to query.</param>
    public CompositeSecretResolver(IEnumerable<ISecretProvider> providers)
    {
        _providers = providers.ToArray();
    }

    /// <inheritdoc />
    public async ValueTask<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        foreach (var provider in _providers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var value = await provider.GetSecretAsync(key, cancellationToken);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }
}

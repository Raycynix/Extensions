using Microsoft.Extensions.Options;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.Abstractions.Records;

namespace Raycynix.Extensions.Secrets.Implementations;

/// <summary>
/// Resolves secrets by querying registered providers in order and returning the first available value.
/// </summary>
public sealed class CompositeSecretResolver : ISecretDiagnosticsResolver
{
    private readonly IReadOnlyCollection<ISecretProvider> _providers;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeSecretResolver"/> class.
    /// </summary>
    /// <param name="providers">The ordered list of secret providers to query.</param>
    /// <param name="options">The secret-resolution options.</param>
    public CompositeSecretResolver(
        IEnumerable<ISecretProvider> providers,
        IOptions<SecretOptions>? options = null)
    {
        _providers = OrderProviders(providers, options?.Value).ToArray();
    }

    /// <inheritdoc />
    public async ValueTask<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        var result = await ResolveSecretAsync(key, cancellationToken);
        return result.Value;
    }

    /// <inheritdoc />
    public async ValueTask<SecretResolutionResult> ResolveSecretAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        foreach (var provider in _providers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var value = await provider.GetSecretAsync(key, cancellationToken);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return new SecretResolutionResult(
                    Key: key,
                    Value: value,
                    ProviderName: provider.GetType().Name);
            }
        }

        return new SecretResolutionResult(key, Value: null, ProviderName: null);
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<SecretResolutionAttempt>> ExplainSecretResolutionAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var attempts = new List<SecretResolutionAttempt>(_providers.Count);
        foreach (var provider in _providers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var value = await provider.GetSecretAsync(key, cancellationToken);
            attempts.Add(new SecretResolutionAttempt(
                ProviderName: provider.GetType().Name,
                Succeeded: !string.IsNullOrWhiteSpace(value)));

            if (!string.IsNullOrWhiteSpace(value))
            {
                break;
            }
        }

        return attempts;
    }

    private static IEnumerable<ISecretProvider> OrderProviders(
        IEnumerable<ISecretProvider> providers,
        SecretOptions? options)
    {
        var providerList = providers.ToList();
        if (options?.ProviderOrder.Count is not > 0)
        {
            return providerList;
        }

        var providerRanks = options.ProviderOrder
            .Select((providerType, index) => new { providerType, index })
            .GroupBy(item => item.providerType)
            .ToDictionary(group => group.Key, group => group.First().index);

        return providerList
            .Select((provider, index) => new
            {
                Provider = provider,
                RegistrationOrder = index,
                Rank = providerRanks.TryGetValue(provider.GetType(), out var rank)
                    ? rank
                    : int.MaxValue
            })
            .OrderBy(item => item.Rank)
            .ThenBy(item => item.RegistrationOrder)
            .Select(item => item.Provider);
    }
}

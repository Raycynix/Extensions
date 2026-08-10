using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Secrets.Options;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.Abstractions.Records;

namespace Raycynix.Extensions.Secrets.Implementations;

/// <summary>
/// Resolves secrets by querying registered providers in order and returning the first available value.
/// </summary>
public sealed class CompositeSecretResolver : ISecretDiagnosticsResolver
{
    private readonly IReadOnlyCollection<ISecretProvider> _providers;
    private readonly bool _continueOnProviderError;
    private readonly ILogger<CompositeSecretResolver>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeSecretResolver"/> class.
    /// </summary>
    /// <param name="providers">The ordered list of secret providers to query.</param>
    /// <param name="options">The secret-resolution options.</param>
    /// <param name="logger">The optional logger used for secret-resolution diagnostics.</param>
    public CompositeSecretResolver(
        IEnumerable<ISecretProvider> providers,
        SecretOptions? options = null,
        ILogger<CompositeSecretResolver>? logger = null)
    {
        _providers = OrderProviders(providers, options).ToArray();
        _continueOnProviderError = options?.ContinueOnProviderError ?? true;
        _logger = logger;

        _logger?.LogDebug(
            "Initialized composite secret resolver. ProviderCount={ProviderCount}.",
            _providers.Count);
    }

    /// <inheritdoc />
    public async ValueTask<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        var diagnostics = await DiagnoseSecretResolutionAsync(key, cancellationToken);
        return diagnostics.Result.Value;
    }

    /// <inheritdoc />
    public async ValueTask<SecretResolutionResult> ResolveSecretAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var diagnostics = await DiagnoseSecretResolutionAsync(key, cancellationToken);
        return diagnostics.Result;
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<SecretResolutionAttempt>> ExplainSecretResolutionAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var diagnostics = await DiagnoseSecretResolutionAsync(key, cancellationToken);
        return diagnostics.Attempts;
    }

    /// <inheritdoc />
    public async ValueTask<SecretResolutionDiagnostics> DiagnoseSecretResolutionAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _logger?.LogDebug(
            "Starting secret resolution. ProviderCount={ProviderCount}.",
            _providers.Count);

        var attempts = new List<SecretResolutionAttempt>(_providers.Count);
        foreach (var provider in _providers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var providerName = provider.GetType().Name;
            _logger?.LogDebug(
                "Trying secret provider. ProviderName={ProviderName}.",
                providerName);

            string? value;
            try
            {
                value = await provider.GetSecretAsync(key, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception) when (_continueOnProviderError)
            {
                attempts.Add(new SecretResolutionAttempt(
                    ProviderName: providerName,
                    Succeeded: false));

                _logger?.LogWarning(
                    "Secret provider failed; resolution will continue with the next provider. ProviderName={ProviderName}, ExceptionType={ExceptionType}.",
                    providerName,
                    exception.GetType().Name);
                continue;
            }

            var succeeded = !string.IsNullOrWhiteSpace(value);
            attempts.Add(new SecretResolutionAttempt(
                ProviderName: providerName,
                Succeeded: succeeded));

            if (succeeded)
            {
                _logger?.LogDebug(
                    "Secret resolved by provider. ProviderName={ProviderName}, AttemptCount={AttemptCount}.",
                    providerName,
                    attempts.Count);

                return new SecretResolutionDiagnostics(
                    new SecretResolutionResult(
                        Key: key,
                        Value: value,
                        ProviderName: providerName),
                    attempts);
            }
        }

        _logger?.LogWarning(
            "Secret resolution failed. AttemptCount={AttemptCount}.",
            attempts.Count);

        return new SecretResolutionDiagnostics(
            new SecretResolutionResult(key, Value: null, ProviderName: null),
            attempts);
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

        var configuredNames = options.ProviderOrder
            .Select(providerName => providerName.Trim())
            .ToArray();

        var unknownNames = configuredNames
            .Where(providerName => providerList.All(provider => !MatchesProvider(provider, providerName)))
            .ToArray();

        if (unknownNames.Length > 0)
        {
            throw new InvalidOperationException(
                $"SecretOptions.ProviderOrder contains providers that are not registered: {string.Join(", ", unknownNames)}.");
        }

        return providerList
            .Select((provider, index) => new
            {
                Provider = provider,
                RegistrationOrder = index,
                Rank = FindProviderRank(provider, configuredNames)
            })
            .OrderBy(item => item.Rank)
            .ThenBy(item => item.RegistrationOrder)
            .Select(item => item.Provider);
    }

    private static int FindProviderRank(ISecretProvider provider, IReadOnlyList<string> configuredNames)
    {
        for (var index = 0; index < configuredNames.Count; index++)
        {
            if (MatchesProvider(provider, configuredNames[index]))
            {
                return index;
            }
        }

        return int.MaxValue;
    }

    private static bool MatchesProvider(ISecretProvider provider, string configuredName)
    {
        var providerType = provider.GetType();
        var shortName = providerType.Name.EndsWith("SecretProvider", StringComparison.Ordinal)
            ? providerType.Name[..^"SecretProvider".Length]
            : providerType.Name;

        return string.Equals(configuredName, shortName, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(configuredName, providerType.Name, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(configuredName, providerType.FullName, StringComparison.Ordinal);
    }
}

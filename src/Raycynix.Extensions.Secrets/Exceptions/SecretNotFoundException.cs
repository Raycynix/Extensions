namespace Raycynix.Extensions.Secrets.Exceptions;

/// <summary>
/// The exception thrown when a required secret cannot be resolved.
/// </summary>
public sealed class SecretNotFoundException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SecretNotFoundException"/> class.
    /// </summary>
    /// <param name="key">The missing secret key.</param>
    /// <param name="providerNames">The providers that were checked.</param>
    public SecretNotFoundException(string key, List<string>? providerNames = null)
        : base(CreateMessage(key, providerNames))
    {
        Key = key;
        ProviderNames = providerNames?.ToList() ?? [];
    }

    /// <summary>
    /// Gets the missing secret key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the providers that were checked.
    /// </summary>
    public IReadOnlyList<string> ProviderNames { get; }

    private static string CreateMessage(string key, List<string>? providerNames)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var providers = providerNames?.ToArray() ?? [];
        if (providers.Length == 0)
        {
            return $"Required secret '{key}' was not found.";
        }

        return $"Required secret '{key}' was not found. Checked providers: {string.Join(", ", providers)}.";
    }
}

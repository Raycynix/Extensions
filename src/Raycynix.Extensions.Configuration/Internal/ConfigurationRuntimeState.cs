using Microsoft.Extensions.Options;

namespace Raycynix.Extensions.Configuration.Internal;

/// <summary>
/// Stores the last approved runtime configuration snapshot for an options type.
/// </summary>
internal sealed class ConfigurationRuntimeState<TOptions>
    where TOptions : class
{
    private readonly Lock _sync = new();

    private readonly Dictionary<string, TOptions> _currentByName = new(StringComparer.OrdinalIgnoreCase);

    public TOptions? GetCurrent(string? name)
    {
        lock (_sync)
            return _currentByName.GetValueOrDefault(NormalizeName(name));
    }

    /// <summary>
    /// Updates the current approved configuration snapshot.
    /// </summary>
    /// <param name="name">The options name associated with the snapshot.</param>
    /// <param name="options">The options instance to store.</param>
    public void SetCurrent(string? name, TOptions? options)
    {
        ArgumentNullException.ThrowIfNull(options);

        lock (_sync)
            _currentByName[NormalizeName(name)] = options;
    }

    private static string NormalizeName(string? name)
    {
        return string.IsNullOrWhiteSpace(name)
            ? Options.DefaultName
            : name;
    }
}

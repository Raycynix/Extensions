namespace Raycynix.Extensions.Configuration.Internal;

/// <summary>
/// Stores the last approved runtime configuration snapshot for an options type.
/// </summary>
internal sealed class ConfigurationRuntimeState<TOptions>
    where TOptions : class
{
    private readonly object _syncRoot = new();
    private TOptions? _current;

    /// <summary>
    /// Gets the current approved configuration snapshot.
    /// </summary>
    public TOptions? Current
    {
        get
        {
            lock (_syncRoot)
            {
                return _current;
            }
        }
    }

    /// <summary>
    /// Updates the current approved configuration snapshot.
    /// </summary>
    /// <param name="options">The options instance to store.</param>
    public void SetCurrent(TOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        lock (_syncRoot)
        {
            _current = options;
        }
    }
}

namespace Raycynix.Extensions.Configuration.Internal;

internal sealed class ConfigurationRuntimeState<TOptions>
    where TOptions : class
{
    private readonly object _syncRoot = new();
    private TOptions? _current;

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

    public void SetCurrent(TOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        lock (_syncRoot)
        {
            _current = options;
        }
    }
}

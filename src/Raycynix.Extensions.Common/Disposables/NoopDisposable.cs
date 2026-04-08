namespace Raycynix.Extensions.Common.Disposables;

/// <summary>
/// Represents a reusable disposable that performs no action when disposed.
/// </summary>
public sealed class NoopDisposable : IDisposable
{
    /// <summary>
    /// Gets the shared singleton instance.
    /// </summary>
    public static NoopDisposable Instance { get; } = new();

    private NoopDisposable()
    {
    }

    /// <inheritdoc />
    public void Dispose()
    {
    }
}

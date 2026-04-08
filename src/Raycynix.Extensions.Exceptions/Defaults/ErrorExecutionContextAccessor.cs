using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions.Defaults;

/// <summary>
/// Stores the current error execution context in an async-local scope.
/// </summary>
internal static class ErrorExecutionContextAccessor
{
    private static readonly AsyncLocal<IErrorExecutionContext?> _current = new();

    /// <summary>
    /// Gets or sets the current error execution context for the active async flow.
    /// </summary>
    public static IErrorExecutionContext? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }
}

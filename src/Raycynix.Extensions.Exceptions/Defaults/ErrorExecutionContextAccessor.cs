using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions.Defaults;

internal static class ErrorExecutionContextAccessor
{
    private static readonly AsyncLocal<IErrorExecutionContext?> _current = new();

    public static IErrorExecutionContext? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }
}

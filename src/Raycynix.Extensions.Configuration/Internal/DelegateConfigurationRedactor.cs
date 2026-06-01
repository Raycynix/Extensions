using Raycynix.Extensions.Configuration.Abstractions.Interfaces;

namespace Raycynix.Extensions.Configuration.Internal;

internal sealed class DelegateConfigurationRedactor(
    Func<string, object?, object?> redact)
    : IConfigurationRedactor
{
    public object? Redact(string key, object? value)
    {
        return redact(key, value);
    }
}

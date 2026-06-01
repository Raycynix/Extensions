using Raycynix.Extensions.Configuration.Abstractions.Interfaces;

namespace Raycynix.Extensions.Configuration.Internal;

/// <summary>
/// Provides the default key-based configuration value redaction rules.
/// </summary>
public sealed class DefaultConfigurationRedactor : IConfigurationRedactor
{
    private static readonly string[] SensitiveFragments =
    [
        "password",
        "passwd",
        "pwd",
        "secret",
        "token",
        "apikey",
        "accesskey",
        "privatekey",
        "signingkey",
        "encryptionkey",
        "connectionstring",
        "connectionstrings",
        "authorization",
        "clientsecret"
    ];

    /// <inheritdoc />
    public object? Redact(string key, object? value)
    {
        if (string.IsNullOrWhiteSpace(key))
            return value;

        var normalizedKey = NormalizeKey(key);

        return SensitiveFragments.Any(fragment =>
            normalizedKey.Contains(fragment, StringComparison.OrdinalIgnoreCase))
            ? "***"
            : value;
    }

    private static string NormalizeKey(string key)
    {
        return new string(key.Where(char.IsLetterOrDigit).ToArray());
    }
}

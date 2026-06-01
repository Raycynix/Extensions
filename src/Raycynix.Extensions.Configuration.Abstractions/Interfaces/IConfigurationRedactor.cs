namespace Raycynix.Extensions.Configuration.Abstractions.Interfaces;

/// <summary>
/// Redacts sensitive configuration values before they are exposed through diagnostics.
/// </summary>
public interface IConfigurationRedactor
{
    /// <summary>
    /// Redacts a configuration value when the provided key is considered sensitive.
    /// </summary>
    /// <param name="key">The configuration key or property path.</param>
    /// <param name="value">The value associated with the key.</param>
    /// <returns>The original value or a redacted replacement.</returns>
    object? Redact(string key, object? value);
}

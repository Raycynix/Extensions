namespace Raycynix.Extensions.Security.Abstractions.Records;

/// <summary>
/// Describes the outcome of resolving a secret through the configured provider chain.
/// </summary>
/// <param name="Key">The secret key that was requested.</param>
/// <param name="Value">The resolved secret value, if any.</param>
/// <param name="ProviderName">The provider type name that returned the value, if any.</param>
public sealed record SecretResolutionResult(string Key, string? Value, string? ProviderName)
{
    /// <summary>
    /// Gets a value indicating whether a non-empty secret value was resolved.
    /// </summary>
    public bool Succeeded => !string.IsNullOrWhiteSpace(Value);
}

namespace Raycynix.Extensions.Security.Abstractions.Records;

/// <summary>
/// Describes a single provider evaluation during secret resolution.
/// </summary>
/// <param name="ProviderName">The provider type name.</param>
/// <param name="Succeeded">Whether the provider returned a non-empty value.</param>
public sealed record SecretResolutionAttempt(string ProviderName, bool Succeeded);

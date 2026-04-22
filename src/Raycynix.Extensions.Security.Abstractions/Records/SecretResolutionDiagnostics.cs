namespace Raycynix.Extensions.Security.Abstractions.Records;

/// <summary>
/// Describes the full diagnostic output for a secret-resolution operation.
/// </summary>
/// <param name="Result">The provider-aware resolution result.</param>
/// <param name="Attempts">The ordered provider attempts that were evaluated.</param>
public sealed record SecretResolutionDiagnostics(
    SecretResolutionResult Result,
    IReadOnlyList<SecretResolutionAttempt> Attempts);

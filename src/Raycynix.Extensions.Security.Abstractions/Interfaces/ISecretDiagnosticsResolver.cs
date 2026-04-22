using Raycynix.Extensions.Security.Abstractions.Records;

namespace Raycynix.Extensions.Security.Abstractions.Interfaces;

/// <summary>
/// Extends secret resolution with provider-aware diagnostics.
/// </summary>
public interface ISecretDiagnosticsResolver : ISecretResolver
{
    /// <summary>
    /// Resolves a secret value and returns the provider attempts evaluated in the same pass.
    /// </summary>
    /// <param name="key">The secret key to resolve.</param>
    /// <param name="cancellationToken">A token for cancelling the operation.</param>
    /// <returns>The full diagnostic output for the resolution operation.</returns>
    ValueTask<SecretResolutionDiagnostics> DiagnoseSecretResolutionAsync(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a secret value and returns provider metadata about the result.
    /// </summary>
    /// <param name="key">The secret key to resolve.</param>
    /// <param name="cancellationToken">A token for cancelling the operation.</param>
    /// <returns>The provider-aware resolution result.</returns>
    ValueTask<SecretResolutionResult> ResolveSecretAsync(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Explains how the configured provider chain evaluated a secret request.
    /// </summary>
    /// <param name="key">The secret key to resolve.</param>
    /// <param name="cancellationToken">A token for cancelling the operation.</param>
    /// <returns>The ordered provider attempts that were evaluated.</returns>
    ValueTask<IReadOnlyList<SecretResolutionAttempt>> ExplainSecretResolutionAsync(
        string key,
        CancellationToken cancellationToken = default);
}

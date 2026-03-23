namespace Raycynix.Extensions.Security.Abstractions.Interfaces;

/// <summary>
/// Represents the aggregated secret resolution entry point used by applications.
/// </summary>
public interface ISecretResolver
{
    /// <summary>
    /// Resolves a secret value using the configured provider chain.
    /// </summary>
    /// <param name="key">The secret key to resolve.</param>
    /// <param name="cancellationToken">A token for cancelling the operation.</param>
    /// <returns>The secret value when found; otherwise <see langword="null" />.</returns>
    ValueTask<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default);
}

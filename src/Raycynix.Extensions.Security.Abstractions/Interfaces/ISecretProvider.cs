namespace Raycynix.Extensions.Security.Abstractions.Interfaces;

/// <summary>
/// Represents a single source of secrets such as environment variables or an external secret store.
/// </summary>
public interface ISecretProvider
{
    /// <summary>
    /// Attempts to read a secret value from the current provider.
    /// </summary>
    /// <param name="key">The secret key to resolve.</param>
    /// <param name="cancellationToken">A token for cancelling the operation.</param>
    /// <returns>The secret value when found; otherwise <see langword="null" />.</returns>
    ValueTask<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default);
}

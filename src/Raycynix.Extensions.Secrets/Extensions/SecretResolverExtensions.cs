using Raycynix.Extensions.Secrets.Exceptions;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.Abstractions.Records;

namespace Raycynix.Extensions.Secrets.Extensions;

/// <summary>
/// Provides convenience extensions for secret resolution.
/// </summary>
public static class SecretResolverExtensions
{
    /// <param name="resolver">The secret resolver to query.</param>
    extension(ISecretResolver resolver)
    {
        /// <summary>
        /// Resolves a secret and throws when the value is missing.
        /// </summary>
        /// <param name="key">The secret key to resolve.</param>
        /// <param name="cancellationToken">A token for cancelling the operation.</param>
        /// <returns>The resolved secret value.</returns>
        /// <exception cref="SecretNotFoundException">Thrown when the secret cannot be resolved.</exception>
        public async ValueTask<string> GetRequiredSecretAsync(string key,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(resolver);
            ArgumentException.ThrowIfNullOrWhiteSpace(key);

            if (resolver is ISecretDiagnosticsResolver diagnosticsResolver)
            {
                var result = await diagnosticsResolver.ResolveSecretAsync(key, cancellationToken);
                if (result.Succeeded)
                {
                    return result.Value!;
                }

                var attempts = await diagnosticsResolver.ExplainSecretResolutionAsync(key, cancellationToken);
                throw new SecretNotFoundException(key, attempts.Select(attempt => attempt.ProviderName).ToList());
            }

            var value = await resolver.GetSecretAsync(key, cancellationToken);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            throw new SecretNotFoundException(key);
        }

        /// <summary>
        /// Resolves a secret together with provider metadata.
        /// </summary>
        /// <param name="key">The secret key to resolve.</param>
        /// <param name="cancellationToken">A token for cancelling the operation.</param>
        /// <returns>The provider-aware resolution result.</returns>
        public async ValueTask<SecretResolutionResult> ResolveWithSourceAsync(string key,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(resolver);
            ArgumentException.ThrowIfNullOrWhiteSpace(key);

            if (resolver is ISecretDiagnosticsResolver diagnosticsResolver)
            {
                return await diagnosticsResolver.ResolveSecretAsync(key, cancellationToken);
            }

            var value = await resolver.GetSecretAsync(key, cancellationToken);
            return new SecretResolutionResult(key, value, ProviderName: null);
        }

        /// <summary>
        /// Explains how the provider chain evaluated the given secret key.
        /// </summary>
        /// <param name="key">The secret key to resolve.</param>
        /// <param name="cancellationToken">A token for cancelling the operation.</param>
        /// <returns>The ordered provider attempts that were evaluated.</returns>
        public async ValueTask<IReadOnlyList<SecretResolutionAttempt>> ExplainSecretResolutionAsync(string key,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(resolver);
            ArgumentException.ThrowIfNullOrWhiteSpace(key);

            if (resolver is ISecretDiagnosticsResolver diagnosticsResolver)
            {
                return await diagnosticsResolver.ExplainSecretResolutionAsync(key, cancellationToken);
            }

            var value = await resolver.GetSecretAsync(key, cancellationToken);
            return
            [
                new SecretResolutionAttempt(
                    ProviderName: resolver.GetType().Name,
                    Succeeded: !string.IsNullOrWhiteSpace(value))
            ];
        }
    }
}

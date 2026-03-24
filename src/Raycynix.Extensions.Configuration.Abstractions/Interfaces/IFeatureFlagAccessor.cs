namespace Raycynix.Extensions.Configuration.Abstractions.Interfaces;

/// <summary>
/// Provides unified access to configured feature flags.
/// </summary>
public interface IFeatureFlagAccessor
{
    /// <summary>
    /// Determines whether the specified feature flag is enabled.
    /// </summary>
    /// <param name="flagName">The feature flag name.</param>
    /// <returns><c>true</c> when the flag is enabled; otherwise, <c>false</c>.</returns>
    bool IsEnabled(string flagName);

    /// <summary>
    /// Determines whether the specified feature flag is disabled.
    /// </summary>
    /// <param name="flagName">The feature flag name.</param>
    /// <returns><c>true</c> when the flag is disabled; otherwise, <c>false</c>.</returns>
    bool IsDisabled(string flagName);

    /// <summary>
    /// Returns all configured feature flags.
    /// </summary>
    /// <returns>A read-only dictionary of feature flag names and values.</returns>
    IReadOnlyDictionary<string, bool> GetAll();
}

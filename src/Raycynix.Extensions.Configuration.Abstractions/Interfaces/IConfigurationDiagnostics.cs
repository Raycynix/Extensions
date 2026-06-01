using Raycynix.Extensions.Configuration.Abstractions.Models;

namespace Raycynix.Extensions.Configuration.Abstractions.Interfaces;

/// <summary>
/// Exposes runtime diagnostics for registered Raycynix configuration options.
/// </summary>
public interface IConfigurationDiagnostics
{
    /// <summary>
    /// Gets the typed configuration registrations known to the diagnostics system.
    /// </summary>
    /// <returns>The registered options metadata.</returns>
    IReadOnlyCollection<ConfigurationRegistrationInfo> GetRegistrations();

    /// <summary>
    /// Gets retained runtime reload results for registered options.
    /// </summary>
    /// <returns>The retained reload diagnostics.</returns>
    IReadOnlyCollection<ConfigurationReloadInfo> GetReloads();

    /// <summary>
    /// Gets the current approved options snapshot with sensitive values redacted.
    /// </summary>
    /// <typeparam name="TOptions">The options model type.</typeparam>
    /// <param name="optionsName">The named options instance to read, or the default options instance when omitted.</param>
    /// <returns>A redacted object graph representing the current approved options snapshot.</returns>
    object? GetRedactedSnapshot<TOptions>(string? optionsName = null)
        where TOptions : class;
}

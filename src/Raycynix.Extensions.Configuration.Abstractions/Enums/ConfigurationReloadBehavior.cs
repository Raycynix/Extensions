namespace Raycynix.Extensions.Configuration.Abstractions.Enums;

/// <summary>
/// Defines whether a runtime configuration change may be applied.
/// </summary>
public enum ConfigurationReloadBehavior
{
    /// <summary>
    /// Applies the change and notifies registered handlers.
    /// </summary>
    Apply = 0,

    /// <summary>
    /// Rejects the change and does not notify registered handlers.
    /// </summary>
    Reject = 1,

    /// <summary>
    /// Ignores the change without replacing the approved snapshot or notifying registered handlers.
    /// </summary>
    Ignore = 2,

    /// <summary>
    /// Keeps the current approved snapshot and indicates that the change requires an application restart.
    /// </summary>
    RestartRequired = 3
}

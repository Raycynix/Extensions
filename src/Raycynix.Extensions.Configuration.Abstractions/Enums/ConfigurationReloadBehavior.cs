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
    Reject = 1
}

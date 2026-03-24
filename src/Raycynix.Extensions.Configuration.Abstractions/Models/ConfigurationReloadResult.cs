using Raycynix.Extensions.Configuration.Abstractions.Enums;

namespace Raycynix.Extensions.Configuration.Abstractions.Models;

/// <summary>
/// Represents the result of evaluating a runtime configuration reload.
/// </summary>
public sealed class ConfigurationReloadResult
{
    private ConfigurationReloadResult(ConfigurationReloadBehavior behavior, string? reason)
    {
        Behavior = behavior;
        Reason = reason;
    }

    /// <summary>
    /// Gets the reload behavior.
    /// </summary>
    public ConfigurationReloadBehavior Behavior { get; }

    /// <summary>
    /// Gets the optional reason for the decision.
    /// </summary>
    public string? Reason { get; }

    /// <summary>
    /// Creates a result that allows the configuration change to be applied.
    /// </summary>
    /// <returns>An allowed reload result.</returns>
    public static ConfigurationReloadResult Apply()
    {
        return new ConfigurationReloadResult(ConfigurationReloadBehavior.Apply, null);
    }

    /// <summary>
    /// Creates a result that rejects the configuration change.
    /// </summary>
    /// <param name="reason">The reason for rejecting the change.</param>
    /// <returns>A rejected reload result.</returns>
    public static ConfigurationReloadResult Reject(string? reason = null)
    {
        return new ConfigurationReloadResult(ConfigurationReloadBehavior.Reject, reason);
    }
}

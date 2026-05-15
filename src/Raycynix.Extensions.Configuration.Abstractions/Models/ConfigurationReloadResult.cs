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

    /// <summary>
    /// Creates a result that ignores the configuration change without treating it as an error.
    /// </summary>
    /// <param name="reason">The optional reason for ignoring the change.</param>
    /// <returns>An ignored reload result.</returns>
    public static ConfigurationReloadResult Ignore(string? reason = null)
    {
        return new ConfigurationReloadResult(ConfigurationReloadBehavior.Ignore, reason);
    }

    /// <summary>
    /// Creates a result that marks the configuration change as requiring an application restart.
    /// </summary>
    /// <param name="reason">The optional reason why a restart is required.</param>
    /// <returns>A restart-required reload result.</returns>
    public static ConfigurationReloadResult RestartRequired(string? reason = null)
    {
        return new ConfigurationReloadResult(ConfigurationReloadBehavior.RestartRequired, reason);
    }
}

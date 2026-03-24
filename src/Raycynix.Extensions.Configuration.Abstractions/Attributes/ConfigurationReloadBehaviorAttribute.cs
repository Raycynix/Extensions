using Raycynix.Extensions.Configuration.Abstractions.Enums;

namespace Raycynix.Extensions.Configuration.Abstractions.Attributes;

/// <summary>
/// Declares how a configuration property should behave during runtime reload.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ConfigurationReloadBehaviorAttribute(ConfigurationReloadBehavior behavior) : Attribute
{
    /// <summary>
    /// Gets the declared reload behavior for the annotated property.
    /// </summary>
    public ConfigurationReloadBehavior Behavior { get; } = behavior;
}

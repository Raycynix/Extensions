using System.Reflection;
using Raycynix.Extensions.Configuration.Abstractions.Attributes;
using Raycynix.Extensions.Configuration.Abstractions.Enums;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;

namespace Raycynix.Extensions.Configuration.Internal;

/// <summary>
/// Enforces runtime reload rules declared with <see cref="ConfigurationReloadBehaviorAttribute"/>.
/// </summary>
internal sealed class AttributeConfigurationReloadPolicy<TOptions> : IConfigurationReloadPolicy<TOptions>
    where TOptions : class
{
    /// <inheritdoc />
    public ConfigurationReloadResult Evaluate(ConfigurationChangeContext<TOptions> context)
    {
        var properties = typeof(TOptions).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var attribute = property.GetCustomAttribute<ConfigurationReloadBehaviorAttribute>();
            if (attribute is null || attribute.Behavior == ConfigurationReloadBehavior.Apply)
            {
                continue;
            }

            var previousValue = property.GetValue(context.Previous);
            var currentValue = property.GetValue(context.Current);

            if (!Equals(previousValue, currentValue))
            {
                return attribute.Behavior switch
                {
                    ConfigurationReloadBehavior.Reject => ConfigurationReloadResult.Reject(
                        $"Property {typeof(TOptions).Name}.{property.Name} cannot be changed at runtime."),
                    ConfigurationReloadBehavior.Ignore => ConfigurationReloadResult.Ignore(),
                    ConfigurationReloadBehavior.RestartRequired => ConfigurationReloadResult.RestartRequired(
                        $"Property {typeof(TOptions).Name}.{property.Name} changes require a restart."),
                    _ => ConfigurationReloadResult.Apply()
                };
            }
        }

        return ConfigurationReloadResult.Apply();
    }
}
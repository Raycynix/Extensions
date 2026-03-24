using System.Reflection;
using Raycynix.Extensions.Configuration.Abstractions.Attributes;
using Raycynix.Extensions.Configuration.Abstractions.Enums;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;

namespace Raycynix.Extensions.Configuration.Internal;

internal sealed class AttributeConfigurationReloadPolicy<TOptions> : IConfigurationReloadPolicy<TOptions>
    where TOptions : class
{
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
                return ConfigurationReloadResult.Reject(
                    $"Property {typeof(TOptions).Name}.{property.Name} cannot be changed at runtime.");
            }
        }

        return ConfigurationReloadResult.Apply();
    }
}

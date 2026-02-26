using System.Reflection;
using Raycynix.Extensions.Database.Abstractions;

namespace Raycynix.Extensions.Database.Implementation;

internal static class ConfiguratorProvider
{
    public static List<IConfigurator?> Provide(Assembly callerAssembly)
    {
        var configuratorTypes = callerAssembly.GetTypes()
            .Where(t => !t.IsAbstract && t is { IsInterface: false, IsClass: true, ReflectedType: null })
            .Where(t => t.GetInterfaces().Any(it => it == typeof(IConfigurator)))
            .ToArray();

        var configurators = configuratorTypes
            .Select(t => Activator.CreateInstance(t) as IConfigurator)
            .ToArray();

        var orderedConfigurators = configurators
            .Where(c => c is { DependsOn.Length: 0 })
            .ToArray();

        while (configurators.Length != orderedConfigurators.Length)
        {
            var readyDependencies = orderedConfigurators.Select(oc => oc?.Type).ToArray();

            var resolvedConfigurators = configurators
                .Where(c => c != null && c.DependsOn.All(t => readyDependencies.Contains(t)))
                .Where(c => orderedConfigurators.All(oc => oc?.Type != c?.Type))
                .ToArray();

            if (resolvedConfigurators.Length == 0) throw new InvalidDataException("Circular dependency!");

            orderedConfigurators = orderedConfigurators.Concat(resolvedConfigurators).ToArray();
        }

        return orderedConfigurators.ToList();
    }
}
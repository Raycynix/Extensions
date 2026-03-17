using System.Reflection;
using Raycynix.Extensions.Database.Abstractions.Configurators;

namespace Raycynix.Extensions.Database.Implementations;

/// <summary>
/// Resolves configurators from an assembly and orders them by declared dependencies.
/// </summary>
internal static class ConfiguratorProvider
{
    /// <summary>
    /// Creates and orders configurators found in the specified assembly.
    /// </summary>
    /// <param name="callerAssembly">The assembly that contains configurator implementations.</param>
    /// <returns>The ordered configurator instances.</returns>
    /// <exception cref="InvalidDataException">Thrown when a circular dependency is detected.</exception>
    public static List<IConfigurator> Provide(Assembly callerAssembly)
    {
        var configuratorTypes = callerAssembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false, IsClass: true })
            .Where(t => typeof(IConfigurator).IsAssignableFrom(t))
            .ToArray();

        var configurators = configuratorTypes
            .Select(t => Activator.CreateInstance(t) as IConfigurator)
            .Where(c => c is not null)
            .Cast<IConfigurator>()
            .ToArray();

        var orderedConfigurators = configurators
            .Where(c => c.DependsOn.Length == 0)
            .ToArray();

        while (configurators.Length != orderedConfigurators.Length)
        {
            var readyDependencies = orderedConfigurators.Select(oc => oc.Type).ToArray();

            var resolvedConfigurators = configurators
                .Where(c => c.DependsOn.All(t => readyDependencies.Contains(t)))
                .Where(c => orderedConfigurators.All(oc => oc.Type != c.Type))
                .ToArray();

            if (resolvedConfigurators.Length == 0)
            {
                throw new InvalidDataException("Circular dependency detected between configurators.");
            }

            orderedConfigurators = orderedConfigurators.Concat(resolvedConfigurators).ToArray();
        }

        return orderedConfigurators.ToList();
    }
}

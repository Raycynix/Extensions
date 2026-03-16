using System.Reflection;
using Raycynix.Extensions.Database.Abstractions.Configurators;

namespace Raycynix.Extensions.Database.Implementations;

/// <summary>
/// Provides utility methods to resolve and order implementations of <see cref="IConfigurator"/>
/// within a given assembly. This is used to configure and seed database entities in a dependency-aware manner.
/// </summary>
/// <remarks>
/// The dependency resolution ensures proper ordering of configurators based on their declared dependencies.
/// Circular dependencies will result in an <see cref="InvalidDataException"/> being thrown.
/// </remarks>
internal static class ConfiguratorProvider
{
    /// <summary>
    /// Resolves and orders implementations of <see cref="IConfigurator"/> defined in the specified assembly.
    /// Ensures that configurators are ordered based on their declared dependencies. Throws an exception
    /// if circular dependencies are detected.
    /// </summary>
    /// <param name="callerAssembly">
    /// The assembly from which the configurators are to be resolved.
    /// </param>
    /// <returns>
    /// A list of <see cref="IConfigurator"/> instances, ordered based on their dependency relationships.
    /// </returns>
    /// <exception cref="InvalidDataException">
    /// Thrown if a circular dependency is detected among the configurators.
    /// </exception>
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
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions.Configurators;

namespace Raycynix.Extensions.Database.Implementations;

/// <summary>
/// Resolves configurators from an assembly and orders them by declared dependencies.
/// </summary>
internal static class ConfiguratorProvider
{
    /// <summary>
    /// Creates and orders configurators found in the specified assemblies.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to activate configurators.</param>
    /// <param name="assemblies">The assemblies that contain configurator implementations.</param>
    /// <returns>The ordered configurator instances.</returns>
    /// <exception cref="InvalidDataException">Thrown when a circular dependency is detected.</exception>
    public static List<IConfigurator> Provide(IServiceProvider serviceProvider, IEnumerable<Assembly> assemblies)
    {
        var configuratorTypes = assemblies
            .Distinct()
            .SelectMany(static assembly => assembly.GetTypes())
            .Where(t => t is { IsAbstract: false, IsInterface: false, IsClass: true })
            .Where(t => typeof(IConfigurator).IsAssignableFrom(t))
            .Distinct()
            .ToArray();

        var configurators = configuratorTypes
            .Select(type => ActivatorUtilities.CreateInstance(serviceProvider, type) as IConfigurator)
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

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions;

namespace Raycynix.Extensions.Database.Implementations;

/// <summary>
/// Stores assemblies that contribute EF Core configurators to the shared database context.
/// </summary>
public sealed class DatabaseModelAssemblyRegistry : IDatabaseModelAssemblyRegistry
{
    private readonly Lock _sync = new();
    private readonly HashSet<Assembly> _assemblies = [];

    /// <summary>
    /// Adds an assembly to the registry.
    /// </summary>
    /// <param name="assembly">The assembly to register.</param>
    public void Add(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        lock (_sync)
            _assemblies.Add(assembly);
    }

    /// <summary>
    /// Gets the registered model assemblies.
    /// </summary>
    /// <returns>The unique assemblies that should be scanned for configurators.</returns>
    public IReadOnlyCollection<Assembly> GetAll()
    {
        lock (_sync)
            return _assemblies.ToArray();
    }
    
    public static DatabaseModelAssemblyRegistry GetOrCreate(IServiceCollection services)
    {
        if (services
                .FirstOrDefault(static descriptor => descriptor.ServiceType == typeof(DatabaseModelAssemblyRegistry))
                ?.ImplementationInstance is DatabaseModelAssemblyRegistry existingRegistry)
        {
            return existingRegistry;
        }

        var registry = new DatabaseModelAssemblyRegistry();
        services.AddSingleton(registry);
        return registry;
    }
}
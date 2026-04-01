using System.Reflection;

namespace Raycynix.Extensions.Database.Implementations;

/// <summary>
/// Stores assemblies that contribute EF Core configurators to the shared database context.
/// </summary>
public sealed class DatabaseModelAssemblyRegistry
{
    private readonly HashSet<Assembly> _assemblies = [];

    /// <summary>
    /// Adds an assembly to the registry.
    /// </summary>
    /// <param name="assembly">The assembly to register.</param>
    public void Add(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        _assemblies.Add(assembly);
    }

    /// <summary>
    /// Gets the registered model assemblies.
    /// </summary>
    /// <returns>The unique assemblies that should be scanned for configurators.</returns>
    public IReadOnlyCollection<Assembly> GetAll()
    {
        return _assemblies.ToArray();
    }
}

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Raycynix.Extensions.Database;

/// <summary>
/// Provides a fluent API for extending the Raycynix database registration.
/// </summary>
public sealed class DatabaseBuilder(IServiceCollection services)
{
    /// <summary>
    /// Gets the underlying service collection.
    /// </summary>
    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));

    /// <summary>
    /// Registers an additional assembly that contributes EF Core configurators to the shared database context.
    /// </summary>
    /// <param name="assembly">The assembly to register.</param>
    /// <returns>The same builder instance.</returns>
    public DatabaseBuilder AddAssembly(Assembly assembly)
    {
        Services.AddRaycynixDatabaseAssembly(assembly);
        return this;
    }

    /// <summary>
    /// Registers an additional assembly that contributes EF Core configurators to the shared database context.
    /// </summary>
    /// <typeparam name="TMarker">A marker type from the assembly to register.</typeparam>
    /// <returns>The same builder instance.</returns>
    public DatabaseBuilder AddAssembly<TMarker>()
    {
        return AddAssembly(typeof(TMarker).Assembly);
    }
}

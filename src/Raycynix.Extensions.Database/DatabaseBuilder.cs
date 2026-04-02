using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Raycynix.Extensions.Database;

/// <summary>
/// Provides a fluent API for extending the Raycynix database registration.
/// </summary>
public class DatabaseBuilder(
    IServiceCollection services,
    IConfiguration configuration,
    Assembly callerAssembly)
{
    /// <summary>
    /// Gets the underlying service collection.
    /// </summary>
    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));

    /// <summary>
    /// Gets the application configuration used for database registrations.
    /// </summary>
    public IConfiguration Configuration { get; } = configuration ?? throw new ArgumentNullException(nameof(configuration));

    /// <summary>
    /// Gets the assembly that initiated the database registration.
    /// </summary>
    public Assembly CallerAssembly { get; } = callerAssembly ?? throw new ArgumentNullException(nameof(callerAssembly));

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

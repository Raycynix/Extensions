using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions;

namespace Raycynix.Extensions.Database;

/// <summary>
/// Provides a fluent API for extending the Raycynix database registration.
/// </summary>
public class DatabaseBuilder(
    IServiceCollection services,
    IConfiguration configuration,
    Assembly callerAssembly) : IDatabaseBuilder
{
    /// <inheritdoc />
    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));

    /// <inheritdoc />
    public IConfiguration Configuration { get; } = configuration ?? throw new ArgumentNullException(nameof(configuration));

    /// <inheritdoc />
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

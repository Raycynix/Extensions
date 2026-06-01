using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Configurations;
using Raycynix.Extensions.Logging.Abstractions;

namespace Raycynix.Extensions.Database;

/// <summary>
/// Provides the default Raycynix EF Core database context.
/// </summary>
public sealed class DatabaseContext : RaycynixDatabaseContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="DatabaseContext"/>.
    /// </summary>
    /// <param name="options">The EF Core options for the context.</param>
    /// <param name="config">The database configuration.</param>
    /// <param name="modelAssemblyRegistry">The registry of assemblies that contain entity configurators.</param>
    /// <param name="observability">The observability hooks used by the Raycynix database infrastructure.</param>
    /// <param name="logger">The logger used during model creation and seeding.</param>
    /// <param name="serviceProvider">The service provider used to activate configurators and resolve database infrastructure services.</param>
    public DatabaseContext(
        DbContextOptions<DatabaseContext> options, 
        DatabaseConfiguration config,
        IDatabaseModelAssemblyRegistry modelAssemblyRegistry,
        IDatabaseObservability observability,
        ILogger<RaycynixDatabaseContext> logger,
        IServiceProvider serviceProvider) 
        : base(options, config, modelAssemblyRegistry, observability, logger, serviceProvider)
    {
    }
}

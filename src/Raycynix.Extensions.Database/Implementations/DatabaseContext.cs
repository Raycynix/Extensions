using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.Configurations;
using Raycynix.Extensions.Logging.Abstractions;

namespace Raycynix.Extensions.Database.Implementations;

/// <summary>
/// Represents the database context for managing interactions with the database.
/// This class is derived from <see cref="DbContext"/> and extends its functionality
/// by integrating custom database configurations and tracking behaviors.
/// </summary>
public sealed class DatabaseContext : DbContext
{
    private readonly ILogger<DatabaseContext> _logger;
    private readonly DatabaseConfiguration _config;
    private readonly Assembly _callerAssembly;

    /// <summary>
    /// Represents a database context that provides functionality for interacting with the database using
    /// Entity Framework Core, with additional configurations for database behavior and logging.
    /// </summary>
    /// <remarks>
    /// This class extends <see cref="DbContext"/> and incorporates custom configurations for managing
    /// database interactions, such as tracking behavior, migrations, seeding, and retry strategies.
    /// Additionally, it integrates a logging mechanism to provide detailed diagnostics and traceability.
    /// </remarks>
    /// <see cref="OnModelCreating"/> is overridden to dynamically load configurations provided by the caller's assembly.
    /// <list type="bullet">
    /// <item>Customizes the behavior of the entity tracker to disable lazy loading and auto-detection of changes.</item>
    /// <item>Query tracking behavior is set to <see cref="QueryTrackingBehavior.NoTracking"/>.</item>
    /// </list>
    public DatabaseContext(DbContextOptions options, DatabaseConfiguration config, Assembly callerAssembly, ILogger<DatabaseContext> logger) 
        : base(options)
    {
        _logger = logger;
        _config = config;
        _callerAssembly = callerAssembly;
        
        ChangeTracker.LazyLoadingEnabled = false;
        ChangeTracker.AutoDetectChangesEnabled = false;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    /// <summary>
    /// Configures the database schema and relationships during the model creation phase of Entity Framework Core.
    /// </summary>
    /// <param name="builder">
    /// An instance of <see cref="ModelBuilder"/> that defines the shape of the entity model and its relationships,
    /// as well as configurations such as constraints and value conversions.
    /// </param>
    /// <remarks>
    /// This method is overridden to dynamically apply entity configurations and seed data during model creation. It
    /// retrieves and invokes configurators from the caller's assembly, allowing for modular and dynamic database schema
    /// definition. If seeding is enabled, it will also execute seed logic for each configurator.
    /// </remarks>
    /// <seealso cref="DatabaseContext"/>
    /// <seealso cref="ConfiguratorProvider"/>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var configurators = ConfiguratorProvider.Provide(_callerAssembly);

        foreach (var configurator in configurators)
        {
            configurator.Configure(builder);

            if (_config.EnableSeed)
            {
                _logger.Information($"Seeding {configurator.GetType().Name}");
                configurator.Seed(builder);
            }
        }
    }
}
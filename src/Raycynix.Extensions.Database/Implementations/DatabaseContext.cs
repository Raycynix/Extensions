using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.Configurations;
using Raycynix.Extensions.Logging.Abstractions;

namespace Raycynix.Extensions.Database.Implementations;

/// <summary>
/// Represents the EF Core database context used by the Raycynix database extensions.
/// </summary>
public sealed class DatabaseContext : DbContext
{
    private readonly ILogger<DatabaseContext> _logger;
    private readonly DatabaseConfiguration _config;
    private readonly Assembly _callerAssembly;

    /// <summary>
    /// Initializes a new instance of <see cref="DatabaseContext"/>.
    /// </summary>
    /// <param name="options">The EF Core options for the context.</param>
    /// <param name="config">The database configuration.</param>
    /// <param name="callerAssembly">The assembly that contains entity configurators.</param>
    /// <param name="logger">The logger used during model creation and seeding.</param>
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
    /// Applies configurators from the caller assembly and optionally registers seed data.
    /// </summary>
    /// <param name="builder">The model builder used to configure the EF Core model.</param>
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

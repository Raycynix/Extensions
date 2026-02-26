using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.Configurations;
using Raycynix.Extensions.Logging.Abstractions;

namespace Raycynix.Extensions.Database.Implementations;

public sealed class DatabaseContext : DbContext
{
    private readonly ILogger<DatabaseContext> _logger;
    private readonly DatabaseConfiguration _config;
    private readonly Assembly _callerAssembly;

    public DatabaseContext(DbContextOptions options, DatabaseConfiguration config, Assembly callerAssembly, ILogger<DatabaseContext> logger) 
        : base(options)
    {
        _logger = logger;
        _config = config;
        _callerAssembly = callerAssembly;
        
        //TODO: Add next to configurations
        ChangeTracker.LazyLoadingEnabled = false;
        ChangeTracker.AutoDetectChangesEnabled = false;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        var configurators = ConfiguratorProvider.Provide(_callerAssembly);
        
        foreach (var configurator in configurators)
        {
            configurator?.Configure(builder);
            if (_config.EnableSeed)
            {
                _logger.Information($"Seeding {configurator?.GetType().Name}");
                configurator?.Seed(builder);
            }
        }
    }
}
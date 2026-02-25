using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.Configurations;

namespace Raycynix.Extensions.Database.Implementations;

/// <summary>
/// 
/// </summary>

//TODO: Create Documentation
public sealed class DatabaseContext : DbContext
{
    private readonly DatabaseConfiguration _config;
    private readonly Assembly _callerAssembly;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="config"></param>
    /// <param name="callerAssembly"></param>
    public DatabaseContext(DbContextOptions options, DatabaseConfiguration config, Assembly callerAssembly) 
        : base(options)
    {
        _config = config;
        _callerAssembly = callerAssembly;
        ChangeTracker.LazyLoadingEnabled = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        var configurators = ConfiguratorProvider.Provide(_callerAssembly);
        foreach (var configurator in configurators)
        {
            configurator?.Configure(builder);
            if (_config.EnableSeed) configurator?.Seed(builder);
        }
    }
}
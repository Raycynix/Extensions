using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Options;
using Raycynix.Extensions.Database.Implementations;

namespace Raycynix.Extensions.Database;

/// <summary>
/// Provides the default EF Core database context used by the Raycynix database infrastructure.
/// </summary>
public sealed class RaycynixDatabaseContext : DbContext, IRaycynixDatabaseContext
{
    private readonly DatabaseOptions _config;
    private readonly IDatabaseModelConfigurator _modelConfigurator;
    private readonly string _providerName;

    /// <summary>
    /// Initializes a new instance of <see cref="RaycynixDatabaseContext"/>.
    /// </summary>
    /// <param name="options">The EF Core options for the context.</param>
    /// <param name="config">The database configuration.</param>
    /// <param name="modelConfigurator">The model configurator used to apply registered entity configurators.</param>
    /// <param name="serviceProvider">The service provider used to resolve database infrastructure services.</param>
    public RaycynixDatabaseContext(
        DbContextOptions options,
        DatabaseOptions config,
        IDatabaseModelConfigurator modelConfigurator,
        IServiceProvider serviceProvider)
        : base(options)
    {
        _config = config;
        _modelConfigurator = modelConfigurator;
        _providerName = serviceProvider.GetRequiredService<DatabaseProviderDescriptor>().ProviderName;

        ConfigureChangeTracker();
    }

    /// <summary>
    /// Applies configurators from the registered model assemblies and optionally registers seed data.
    /// </summary>
    /// <param name="builder">The model builder used to configure the EF Core model.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        _modelConfigurator.Configure(builder, _providerName);
    }

    private void ConfigureChangeTracker()
    {
        ChangeTracker.LazyLoadingEnabled = _config.EnableLazyLoading;
        ChangeTracker.AutoDetectChangesEnabled = _config.EnableAutoDetectChanges;
        ChangeTracker.QueryTrackingBehavior = _config.UseQueryTrackingByDefault
            ? QueryTrackingBehavior.TrackAll
            : QueryTrackingBehavior.NoTracking;
    }

    /// <inheritdoc />
    public string GetModelCacheKey()
    {
        return _modelConfigurator.GetModelCacheKey(_providerName);
    }
}

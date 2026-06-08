using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Configurations;
using Raycynix.Extensions.Database.Implementations;

namespace Raycynix.Extensions.Database.AspNetCore.Identity;

public sealed class RaycynixIdentityDatabaseContext : IdentityDbContext, IRaycynixDatabaseContext
{
    private readonly DatabaseConfiguration _config;
    private readonly IDatabaseModelConfigurator _modelConfigurator;
    private readonly string _providerName;

    public RaycynixIdentityDatabaseContext(
        DbContextOptions options,
        DatabaseConfiguration config,
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

    public string GetModelCacheKey()
    {
        return _modelConfigurator.GetModelCacheKey(_providerName);
    }
}
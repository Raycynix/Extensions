using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Configurations;
using Raycynix.Extensions.Database.Abstractions.Configurators;
using Raycynix.Extensions.Database.Implementations;
using Raycynix.Extensions.Database.Internal;
using Raycynix.Extensions.Logging.Abstractions;

namespace Raycynix.Extensions.Database;

/// <summary>
/// Provides the extensible EF Core database context used by the Raycynix database infrastructure.
/// </summary>
public abstract class RaycynixDatabaseContext : DbContext
{
    private readonly ILogger<RaycynixDatabaseContext> _logger;
    private readonly DatabaseConfiguration _config;
    private readonly IDatabaseModelAssemblyRegistry _modelAssemblyRegistry;
    private readonly IDatabaseObservability _observability;
    private readonly string _providerName;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of <see cref="RaycynixDatabaseContext"/>.
    /// </summary>
    /// <param name="options">The EF Core options for the context.</param>
    /// <param name="config">The database configuration.</param>
    /// <param name="modelAssemblyRegistry">The registry of assemblies that contain entity configurators.</param>
    /// <param name="observability">The observability hooks used during model creation.</param>
    /// <param name="logger">The logger used during model creation and seeding.</param>
    /// <param name="serviceProvider">The service provider used to activate configurators and resolve database infrastructure services.</param>
    protected RaycynixDatabaseContext(
        DbContextOptions options,
        DatabaseConfiguration config,
        IDatabaseModelAssemblyRegistry modelAssemblyRegistry,
        IDatabaseObservability observability,
        ILogger<RaycynixDatabaseContext> logger,
        IServiceProvider serviceProvider)
        : base(options)
    {
        _logger = logger;
        _config = config;
        _modelAssemblyRegistry = modelAssemblyRegistry;
        _observability = observability;
        _providerName = serviceProvider.GetRequiredService<DatabaseProviderDescriptor>().ProviderName;
        _serviceProvider = serviceProvider;
        
        ConfigureChangeTracker();
    }

    /// <summary>
    /// Applies configurators from the registered model assemblies and optionally registers seed data.
    /// </summary>
    /// <param name="builder">The model builder used to configure the EF Core model.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        using var modelCreatingScope = _observability.BeginOperation(_providerName, "model_creating");

        var configurators = GetConfigurators();
        _observability.AddTag("database.configurator.count", configurators.Count.ToString());

        foreach (var configurator in configurators)
        {
            configurator.Configure(builder);

            if (_config.EnableSeed)
            {
                _logger.Information($"Seeding {configurator.GetType().Name}");
                configurator.Seed(builder);
            }
        }

        _observability.RecordSuccess(_providerName, "model_creating");
    }

    /// <summary>
    /// Builds the cache key fragment representing the active provider, seed mode, and applied configurators.
    /// </summary>
    /// <returns>The model cache key fragment for the current context instance.</returns>
    internal string GetModelCacheKey()
    {
        var configuratorKeys = GetConfigurators()
            .Select(static configurator => configurator.ModelCacheKey)
            .OrderBy(static key => key, StringComparer.Ordinal)
            .ToArray();

        return string.Join(
            "|",
            new[] { _providerName, _config.EnableSeed.ToString() }
                .Concat(configuratorKeys));
    }

    private List<IConfigurator> GetConfigurators()
    {
        return ConfiguratorProvider.Provide(_serviceProvider, _modelAssemblyRegistry.GetAll());
    }

    private void ConfigureChangeTracker()
    {
        ChangeTracker.LazyLoadingEnabled = _config.EnableLazyLoading;
        ChangeTracker.AutoDetectChangesEnabled = _config.EnableAutoDetectChanges;
        ChangeTracker.QueryTrackingBehavior = _config.UseQueryTrackingByDefault
            ? QueryTrackingBehavior.TrackAll
            : QueryTrackingBehavior.NoTracking;
    }
}

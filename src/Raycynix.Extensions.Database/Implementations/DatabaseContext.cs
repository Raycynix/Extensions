using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Configurations;
using Raycynix.Extensions.Database.Abstractions.Configurators;
using Raycynix.Extensions.Database.Internal;
using Raycynix.Extensions.Logging.Abstractions;

namespace Raycynix.Extensions.Database.Implementations;

/// <summary>
/// Represents the EF Core database context used by the Raycynix database extensions.
/// </summary>
public sealed class DatabaseContext : DbContext
{
    private readonly ILogger<DatabaseContext> _logger;
    private readonly DatabaseConfiguration _config;
    private readonly DatabaseModelAssemblyRegistry _modelAssemblyRegistry;
    private readonly DatabaseObservability _observability;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of <see cref="DatabaseContext"/>.
    /// </summary>
    /// <param name="options">The EF Core options for the context.</param>
    /// <param name="config">The database configuration.</param>
    /// <param name="modelAssemblyRegistry">The registry of assemblies that contain entity configurators.</param>
    /// <param name="logger">The logger used during model creation and seeding.</param>
    /// <param name="serviceProvider">The service provider used to resolve optional observability integrations.</param>
    public DatabaseContext(
        DbContextOptions options,
        DatabaseConfiguration config,
        DatabaseModelAssemblyRegistry modelAssemblyRegistry,
        ILogger<DatabaseContext> logger,
        IServiceProvider serviceProvider)
        : base(options)
    {
        _logger = logger;
        _config = config;
        _modelAssemblyRegistry = modelAssemblyRegistry;
        _observability = serviceProvider.GetRequiredService<DatabaseObservability>();
        _serviceProvider = serviceProvider;
        
        ChangeTracker.LazyLoadingEnabled = config.EnableLazyLoading;
        ChangeTracker.AutoDetectChangesEnabled = config.EnableAutoDetectChanges;
        ChangeTracker.QueryTrackingBehavior = config.UseQueryTrackingByDefault
            ? QueryTrackingBehavior.TrackAll
            : QueryTrackingBehavior.NoTracking;
    }

    /// <summary>
    /// Applies configurators from the registered model assemblies and optionally registers seed data.
    /// </summary>
    /// <param name="builder">The model builder used to configure the EF Core model.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        using var modelCreatingScope = _observability.BeginOperation(_config.Provider, "model_creating");

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

        _observability.RecordSuccess(_config.Provider, "model_creating");
    }

    internal string GetModelCacheKey()
    {
        var configuratorKeys = GetConfigurators()
            .Select(static configurator => configurator.ModelCacheKey)
            .OrderBy(static key => key, StringComparer.Ordinal)
            .ToArray();

        return string.Join(
            "|",
            new[] { _config.Provider.ToString(), _config.EnableSeed.ToString() }
                .Concat(configuratorKeys));
    }

    private List<IConfigurator> GetConfigurators()
    {
        return ConfiguratorProvider.Provide(_serviceProvider, _modelAssemblyRegistry.GetAll());
    }
}

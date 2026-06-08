using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Configurations;
using Raycynix.Extensions.Database.Abstractions.Configurators;
using Raycynix.Extensions.Logging.Abstractions;

namespace Raycynix.Extensions.Database.Implementations;

public sealed class DatabaseModelConfigurator(
    ILogger<DatabaseModelConfigurator> logger,
    DatabaseConfiguration config,
    IDatabaseModelAssemblyRegistry modelAssemblyRegistry,
    IDatabaseObservability observability,
    IServiceProvider serviceProvider)
    : IDatabaseModelConfigurator
{
    public void Configure(ModelBuilder modelBuilder, string providerName)
    {
        try
        {
            using var modelCreatingScope = observability.BeginOperation(providerName, "model_creating");

            var configurators = GetConfigurators();
            observability.AddTag("database.configurator.count", configurators.Count.ToString());

            foreach (var configurator in configurators)
            {
                configurator.Configure(modelBuilder);

                if (config.EnableSeed)
                {
                    logger.Information("Seeding {Name}", configurator.GetType().Name);
                    configurator.Seed(modelBuilder);
                }
            }

            observability.RecordSuccess(providerName, "model_creating");

        }
        catch (Exception ex)
        {
            observability.RecordFailure(providerName, "model_creating");
            throw new Exception("Failed to configure database model", ex);
            
        }
    }
    
    private List<IConfigurator> GetConfigurators()
    {
        return ConfiguratorProvider.Provide(serviceProvider, modelAssemblyRegistry.GetAll());
    }
    
    
    /// <summary>
    /// Builds the cache key fragment representing the active provider, seed mode, and applied configurators.
    /// </summary>
    /// <returns>The model cache key fragment for the current context instance.</returns>
    public string GetModelCacheKey(string providerName)
    {
        var configuratorKeys = GetConfigurators()
            .Select(static configurator => configurator.ModelCacheKey)
            .OrderBy(static key => key, StringComparer.Ordinal)
            .ToArray();

        return string.Join(
            "|",
            new[] { providerName, config.EnableSeed.ToString() }
                .Concat(configuratorKeys));
    }
}
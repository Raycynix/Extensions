using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Configurations;
using Raycynix.Extensions.Logging.Abstractions;

namespace Raycynix.Extensions.Database.Implementations;

/// <summary>
/// Initializes the database by creating it and/or applying migrations, depending on the configuration.
/// </summary>
public class DatabaseInitializer<TContext>(
    IServiceScopeFactory serviceScopeFactory,
    IDatabaseObservability observability,
    DatabaseConfiguration config,
    DatabaseProviderDescriptor descriptor,
    ILogger<DatabaseInitializer<TContext>> logger) : IDatabaseInitializer
    where TContext : DbContext, IRaycynixDatabaseContext
{
    private readonly SemaphoreSlim _lock = new(1, 1);

    private readonly string _providerName = descriptor.ProviderName;

    /// <summary>
    /// Gets a value indicating whether initialization has already completed.
    /// </summary>
    public bool IsReady { get; private set; }

    /// <summary>
    /// Runs the configured database initialization steps at once.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (IsReady)
        {
            logger.Information("Database is already initialized");
            return;
        }

        await _lock.WaitAsync(cancellationToken);

        try
        {
            if (IsReady)
            {
                logger.Information("Database is already initialized");
                return;
            }

            logger.Information("Starting database initializer");
            using var initializationScope = observability.BeginOperation(_providerName, "initialization");

            using var scope = serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TContext>();

            if (config.EnsureCreated)
            {
                logger.Information("Applying database creation");
                using var ensureCreatedScope = observability.BeginOperation(_providerName, "ensure_created");

                try
                {
                    await context.Database.EnsureCreatedAsync(cancellationToken);
                    observability.RecordSuccess(_providerName, "ensure_created");
                }
                catch
                {
                    observability.RecordFailure(_providerName, "ensure_created");
                    throw;
                }
            }

            if (config.UseMigrations)
            {
                logger.Information("Applying migrations");
                using var migrationsScope = observability.BeginOperation(_providerName, "migrate");

                try
                {
                    await context.Database.MigrateAsync(cancellationToken);
                    observability.RecordSuccess(_providerName, "migrate");
                }
                catch
                {
                    observability.RecordFailure(_providerName, "migrate");
                    throw;
                }
            }

            IsReady = true;
            observability.RecordSuccess(_providerName, "initialization");
            logger.Information("Database initialized");
        }
        catch
        {
            observability.RecordFailure(_providerName, "initialization");
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }
}
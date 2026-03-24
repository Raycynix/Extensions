using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Configurations;
using Raycynix.Extensions.Database.Internal;
using Raycynix.Extensions.Logging.Abstractions;

namespace Raycynix.Extensions.Database.Implementations;

/// <summary>
/// Initializes the database by creating it and/or applying migrations, depending on the configuration.
/// </summary>
public class DatabaseInitializer(
    IServiceProvider serviceProvider,
    DatabaseConfiguration config,
    ILogger<DatabaseInitializer> logger) : IDatabaseInitializer
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly DatabaseObservability _observability = serviceProvider.GetRequiredService<DatabaseObservability>();

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
            using var initializationScope = _observability.BeginOperation(config.Provider, "initialization");

            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            if (config.EnsureCreated)
            {
                logger.Information("Applying database creation");
                using var ensureCreatedScope = _observability.BeginOperation(config.Provider, "ensure_created");

                try
                {
                    await context.Database.EnsureCreatedAsync(cancellationToken);
                    _observability.RecordSuccess(config.Provider, "ensure_created");
                }
                catch
                {
                    _observability.RecordFailure(config.Provider, "ensure_created");
                    throw;
                }
            }

            if (config.UseMigrations)
            {
                logger.Information("Applying migrations");
                using var migrationsScope = _observability.BeginOperation(config.Provider, "migrate");

                try
                {
                    await context.Database.MigrateAsync(cancellationToken);
                    _observability.RecordSuccess(config.Provider, "migrate");
                }
                catch
                {
                    _observability.RecordFailure(config.Provider, "migrate");
                    throw;
                }
            }

            IsReady = true;
            _observability.RecordSuccess(config.Provider, "initialization");
            logger.Information("Database initialized");
        }
        catch
        {
            _observability.RecordFailure(config.Provider, "initialization");
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }
}

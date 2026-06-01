using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Configurations;
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
    private readonly IDatabaseObservability _observability = serviceProvider.GetRequiredService<IDatabaseObservability>();
    private readonly string _providerName = serviceProvider.GetRequiredService<DatabaseProviderDescriptor>().ProviderName;

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
            using var initializationScope = _observability.BeginOperation(_providerName, "initialization");

            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RaycynixDatabaseContext>();

            if (config.EnsureCreated)
            {
                logger.Information("Applying database creation");
                using var ensureCreatedScope = _observability.BeginOperation(_providerName, "ensure_created");

                try
                {
                    await context.Database.EnsureCreatedAsync(cancellationToken);
                    _observability.RecordSuccess(_providerName, "ensure_created");
                }
                catch
                {
                    _observability.RecordFailure(_providerName, "ensure_created");
                    throw;
                }
            }

            if (config.UseMigrations)
            {
                logger.Information("Applying migrations");
                using var migrationsScope = _observability.BeginOperation(_providerName, "migrate");

                try
                {
                    await context.Database.MigrateAsync(cancellationToken);
                    _observability.RecordSuccess(_providerName, "migrate");
                }
                catch
                {
                    _observability.RecordFailure(_providerName, "migrate");
                    throw;
                }
            }

            IsReady = true;
            _observability.RecordSuccess(_providerName, "initialization");
            logger.Information("Database initialized");
        }
        catch
        {
            _observability.RecordFailure(_providerName, "initialization");
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }
}

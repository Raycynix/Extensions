using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Configurations;
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

            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            if (config.EnsureCreated)
            {
                logger.Information("Applying database creation");
                await context.Database.EnsureCreatedAsync(cancellationToken);
            }

            if (config.UseMigrations)
            {
                logger.Information("Applying migrations");
                await context.Database.MigrateAsync(cancellationToken);
            }

            IsReady = true;
            logger.Information("Database initialized");
        }
        finally
        {
            _lock.Release();
        }
    }
}

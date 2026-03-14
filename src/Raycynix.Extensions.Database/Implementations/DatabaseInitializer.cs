using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Configurations;
using Raycynix.Extensions.Logging.Abstractions;

namespace Raycynix.Extensions.Database.Implementations;

/// <summary>
/// Handles the initialization of the database, including optional creation and applying migrations based on the provided configuration.
/// </summary>
/// <remarks>
/// This class is responsible for preparing the database during application startup by ensuring the database is created
/// and migrations are applied, if necessary. It also provides a flag to indicate whether the database is ready for use.
/// </remarks>
public class DatabaseInitializer(
    IServiceProvider serviceProvider,
    DatabaseConfiguration config,
    ILogger<DatabaseInitializer> logger) : IDatabaseInitializer
{
    /// <summary>
    /// Indicates whether the database has been initialized and is ready for use.
    /// </summary>
    /// <remarks>
    /// This property will be set to true after the database initialization process has successfully completed.
    /// It remains false until the initialization process is finalized within the implementation of the database initializer.
    /// </remarks>
    public bool IsReady { get; private set; }

    /// <summary>
    /// Initializes the database by creating it if required, applying migrations, and marking it as ready.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        logger.Information("Starting database initializer");

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetService<DatabaseContext>();

        if (context is null)
        {
            logger.Fatal("Database context is null");
            return;
        }
        
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
}
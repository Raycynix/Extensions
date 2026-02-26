using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Configurations;
using Raycynix.Extensions.Logging.Abstractions;

namespace Raycynix.Extensions.Database.Implementations;

public class DatabaseInitializer(
    IServiceProvider serviceProvider,
    DatabaseConfiguration config,
    ILogger<DatabaseInitializer> logger) : IDatabaseInitializer
{
    public bool IsReady { get; private set; }

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
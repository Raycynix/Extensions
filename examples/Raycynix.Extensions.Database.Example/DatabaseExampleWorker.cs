using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Database.Implementations;
using Raycynix.Extensions.Logging.Abstractions;

namespace Raycynix.Extensions.Database.Example;

internal sealed class DatabaseExampleWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<DatabaseExampleWorker> logger,
    IHostApplicationLifetime applicationLifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        logger.Information("Database example started");

        var existingOrder = await databaseContext.Set<ExampleOrder>()
            .FirstOrDefaultAsync(current => current.Number == "ORD-2026-0002", stoppingToken);

        if (existingOrder is null)
        {
            databaseContext.Set<ExampleOrder>().Add(new ExampleOrder
            {
                Id = Guid.NewGuid(),
                Number = "ORD-2026-0002",
                CustomerName = "Alice Johnson",
                TotalAmount = 249.99m,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            });

            await databaseContext.SaveChangesAsync(stoppingToken);
            logger.Information("Created a new order", new { OrderNumber = "ORD-2026-0002" });
        }

        var orders = await databaseContext.Set<ExampleOrder>()
            .AsNoTracking()
            .OrderBy(current => current.CreatedAt)
            .ToListAsync(stoppingToken);

        foreach (var order in orders)
        {
            logger.Information("Loaded order from database", new
            {
                order.Number,
                order.CustomerName,
                order.TotalAmount,
                order.Status,
                order.CreatedAt
            });
        }

        logger.Information("Database example finished", new { Count = orders.Count });
        applicationLifetime.StopApplication();
    }
}
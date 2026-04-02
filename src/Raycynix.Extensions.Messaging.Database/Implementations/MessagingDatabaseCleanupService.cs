using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Messaging.Database.Configurations;

namespace Raycynix.Extensions.Messaging.Database.Implementations;

/// <summary>
/// Runs periodic retention cleanup for messaging inbox and outbox tables.
/// </summary>
internal sealed class MessagingDatabaseCleanupService(
    IServiceScopeFactory serviceScopeFactory,
    MessagingDatabasePersistenceConfiguration configuration,
    ILogger<MessagingDatabaseCleanupService> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!configuration.EnableCleanup)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = serviceScopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<MessagingDatabaseCleanupProcessor>();
                await processor.ProcessAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "An error occurred while cleaning messaging database persistence.");
            }

            await Task.Delay(configuration.CleanupInterval, stoppingToken).ConfigureAwait(false);
        }
    }
}

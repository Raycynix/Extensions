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
    ILogger<MessagingDatabaseCleanupService>? logger = null) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!configuration.EnableCleanup)
        {
            logger?.LogDebug("Messaging database cleanup is disabled.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = serviceScopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<MessagingDatabaseCleanupProcessor>();
                var deleted = await processor.ProcessAsync(stoppingToken).ConfigureAwait(false);
                logger?.LogDebug("Messaging database cleanup cycle completed. DeletedCount={DeletedCount}.", deleted);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger?.LogError(exception, "An error occurred while cleaning messaging database persistence.");
            }

            await Task.Delay(configuration.CleanupInterval, stoppingToken).ConfigureAwait(false);
        }
    }
}
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Messaging.Configurations;

namespace Raycynix.Extensions.Messaging.Implementations;

/// <summary>
/// Runs background recovery for pending and failed outbox messages.
/// </summary>
internal sealed class MessageOutboxRecoveryService(
    IServiceScopeFactory serviceScopeFactory,
    MessagingConfiguration configuration,
    ILogger<MessageOutboxRecoveryService>? logger = null) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (configuration.Outbox is { Enabled: true, EnableRecovery: true })
                {
                    logger?.LogDebug("Starting messaging outbox recovery cycle.");
                    await using var scope = serviceScopeFactory.CreateAsyncScope();
                    var processor = scope.ServiceProvider.GetRequiredService<MessageOutboxRecoveryProcessor>();
                    var recovered = await processor.ProcessAvailableAsync(stoppingToken).ConfigureAwait(false);
                    logger?.LogDebug("Messaging outbox recovery cycle completed. RecoveredCount={RecoveredCount}.",
                        recovered);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger?.LogError(exception, "An error occurred while recovering outbox messages.");
            }

            await Task.Delay(configuration.Outbox.RecoveryInterval, stoppingToken).ConfigureAwait(false);
        }
    }
}
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Messaging.Configurations;

namespace Raycynix.Extensions.Messaging.Implementations;

/// <summary>
/// Runs background recovery for pending and failed outbox messages.
/// </summary>
internal sealed class MessageOutboxRecoveryService(
    MessageOutboxRecoveryProcessor processor,
    MessagingConfiguration configuration,
    ILogger<MessageOutboxRecoveryService> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (configuration.Outbox.Enabled && configuration.Outbox.EnableRecovery)
                {
                    await processor.ProcessAvailableAsync(stoppingToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "An error occurred while recovering outbox messages.");
            }

            await Task.Delay(configuration.Outbox.RecoveryInterval, stoppingToken).ConfigureAwait(false);
        }
    }
}

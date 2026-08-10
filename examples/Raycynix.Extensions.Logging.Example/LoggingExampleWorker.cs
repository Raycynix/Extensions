using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Serilog.Configurations;

namespace Raycynix.Extensions.Logging.Example;

internal sealed class LoggingExampleWorker(
    ILogger<LoggingExampleWorker> logger,
    OrderProcessor orderProcessor,
    RaycynixSerilogOptions raycynixSerilogOptions,
    IHostApplicationLifetime applicationLifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Logging example started for {ServiceName} in {Environment}",
            raycynixSerilogOptions.ServiceName,
            raycynixSerilogOptions.Environment);

        using (logger.BeginScope(new Dictionary<string, object>
               {
                   ["Scenario"] = "ExampleBootstrap",
                   ["CorrelationId"] = Guid.NewGuid()
               }))
        {
            logger.LogTrace("Trace log with structured metadata {@Metadata}", new
            {
                Step = "Bootstrap",
                Timestamp = DateTimeOffset.UtcNow
            });

            logger.LogDebug("Debug log with startup details {@Metadata}", new
            {
                Environment.MachineName,
                Environment.ProcessId
            });

            await orderProcessor.ProcessAsync("ORD-2026-0001", stoppingToken);
        }

        logger.LogInformation("Logging example finished");
        applicationLifetime.StopApplication();
    }
}

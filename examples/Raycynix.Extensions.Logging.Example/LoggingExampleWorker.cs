using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Logging.Abstractions;
using Raycynix.Extensions.Logging.Abstractions.Configurations;

namespace Raycynix.Extensions.Logging.Example;

internal sealed class LoggingExampleWorker(
    ILogger<LoggingExampleWorker> logger,
    OrderProcessor orderProcessor,
    IOptions<LoggingConfiguration> loggingConfiguration,
    IHostApplicationLifetime applicationLifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var configuration = loggingConfiguration.Value;

        logger.Information("Logging example started with environment {Environment}", new
        {
            configuration.ServiceName,
            configuration.Environment,
            configuration.MinimumLevel
        });

        using (logger.BeginScope(new Dictionary<string, object>
               {
                   ["Scenario"] = "ExampleBootstrap",
                   ["CorrelationId"] = Guid.NewGuid()
               }))
        {
            logger.Trace("Trace log with structured metadata {@Metadata}", new
            {
                Step = "Bootstrap",
                Timestamp = DateTimeOffset.UtcNow
            });

            logger.Debug("Debug log with startup details {@Metadata}", new
            {
                Environment.MachineName,
                Environment.ProcessId
            });

            await orderProcessor.ProcessAsync("ORD-2026-0001", stoppingToken);
        }

        logger.Information("Logging example finished");
        applicationLifetime.StopApplication();
    }
}
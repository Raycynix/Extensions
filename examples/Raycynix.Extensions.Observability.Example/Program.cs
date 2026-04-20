using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Logging;
using Raycynix.Extensions.Observability;
using Raycynix.Extensions.Observability.Example;

Environment.CurrentDirectory = AppContext.BaseDirectory;

using var activityListener = new ActivityListener();
activityListener.ShouldListenTo = static _ => true;
activityListener.Sample = static (ref _) => ActivitySamplingResult.AllDataAndRecorded;
activityListener.SampleUsingParentId = static (ref _) => ActivitySamplingResult.AllDataAndRecorded;

ActivitySource.AddActivityListener(activityListener);

var builder = Host.CreateDefaultBuilder(args);

builder
    .UseRaycynixLogging(options =>
    {
        options.MinimumLevel = Microsoft.Extensions.Logging.LogLevel.Debug;
        options.OutputTemplate =
            "[{Timestamp:HH:mm:ss}] [{Level:u3}] [{ServiceName}] [{ServiceVersion}] [Env:{Environment}] [Trace:{TraceId}] [Corr:{CorrelationId}] {Message:lj}{NewLine}{Exception}";
    })
    .ConfigureServices((context, services) =>
    {
        services.AddRaycynixObservability();
        services.AddRaycynixLogging(context.Configuration);
        services.AddHostedService<ObservabilityExampleWorker>();
    });

await builder.RunConsoleAsync();
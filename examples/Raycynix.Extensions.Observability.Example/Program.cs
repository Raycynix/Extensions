using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Observability;
using Raycynix.Extensions.Observability.Example;

Environment.CurrentDirectory = AppContext.BaseDirectory;

using var activityListener = new ActivityListener();
activityListener.ShouldListenTo = static _ => true;
activityListener.Sample = static (ref _) => ActivitySamplingResult.AllDataAndRecorded;
activityListener.SampleUsingParentId = static (ref _) => ActivitySamplingResult.AllDataAndRecorded;

ActivitySource.AddActivityListener(activityListener);

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices(services =>
{
    services.AddRaycynixObservability();
    services.AddHostedService<ObservabilityExampleWorker>();
});

await builder.RunConsoleAsync();
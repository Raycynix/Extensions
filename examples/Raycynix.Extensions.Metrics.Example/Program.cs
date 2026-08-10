using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Metrics;
using Raycynix.Extensions.Metrics.Abstractions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRaycynixMetrics();
builder.Services.AddHostedService<MetricsExampleWorker>();

await builder.Build().RunAsync();

internal sealed class MetricsExampleWorker(
    IMeterFactory meterFactory,
    IHostApplicationLifetime applicationLifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var meter = RaycynixMetrics.CreateMeter(meterFactory);
        var processedOrders = meter.CreateCounter<long>(
            "raycynix.example.orders.processed",
            unit: "{order}",
            description: "Number of processed orders.");
        var activeWorkers = meter.CreateUpDownCounter<long>(
            "raycynix.example.workers.active",
            unit: "{worker}",
            description: "Change in active workers.");
        var processingDuration = meter.CreateHistogram<double>(
            "raycynix.example.order.processing.duration",
            unit: "s",
            description: "Order processing duration.");

        activeWorkers.Add(
            1,
            new KeyValuePair<string, object?>("raycynix.worker.name", "orders-worker"));

        using (processingDuration.MeasureDuration(
                   new KeyValuePair<string, object?>("raycynix.operation", "process_order")))
        {
            await Task.Delay(150, stoppingToken);
            processedOrders.Add(
                1,
                new KeyValuePair<string, object?>("raycynix.status", "success"));
        }

        Console.WriteLine("Recorded standard System.Diagnostics.Metrics instruments.");
        Console.WriteLine("Attach an OpenTelemetry exporter in the host to collect them.");

        activeWorkers.Add(
            -1,
            new KeyValuePair<string, object?>("raycynix.worker.name", "orders-worker"));
        applicationLifetime.StopApplication();
    }
}

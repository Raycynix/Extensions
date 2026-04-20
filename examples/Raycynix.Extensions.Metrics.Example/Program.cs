using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Metrics;
using Raycynix.Extensions.Metrics.Abstractions.Interfaces;

Environment.CurrentDirectory = AppContext.BaseDirectory;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRaycynixMetrics(builder.Configuration, options =>
{
    options.UsePrometheus = true;
    options.MetricsEndpoint = "/metrics";
    options.UseHealthChecks = true;
});
builder.Services.AddHostedService<MetricsExampleWorker>();

await builder.Build().RunAsync();

internal sealed class MetricsExampleWorker(
    IMetricsService metricsService,
    IHostApplicationLifetime applicationLifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var processedOrdersCounter = metricsService.CreateCounter(
            "raycynix_example_processed_orders_total",
            "Number of processed orders.",
            "status");

        var activeWorkersGauge = metricsService.CreateGauge(
            "raycynix_example_active_workers",
            "Current number of active workers.",
            "worker");

        var processingDurationHistogram = metricsService.CreateHistogram(
            "raycynix_example_order_processing_seconds",
            "Order processing duration in seconds.",
            "operation");

        Console.WriteLine("Raycynix metrics example");
        Console.WriteLine("Creating sample metrics and recording values...");

        activeWorkersGauge.Set(1, "orders-worker");

        using (processingDurationHistogram.MeasureDuration("process_order"))
        {
            await Task.Delay(150, stoppingToken);
            processedOrdersCounter.Increment(labelValues: ["success"]);
        }

        using (processingDurationHistogram.MeasureDuration("process_order"))
        {
            await Task.Delay(75, stoppingToken);
            processedOrdersCounter.Increment(labelValues: ["success"]);
        }

        processingDurationHistogram.Observe(0.42, "recalculate_totals");
        activeWorkersGauge.Increment(1, "orders-worker");
        activeWorkersGauge.Decrement(1, "orders-worker");

        Console.WriteLine("Metrics recorded:");
        Console.WriteLine("  counter: raycynix_example_processed_orders_total");
        Console.WriteLine("  gauge: raycynix_example_active_workers");
        Console.WriteLine("  histogram: raycynix_example_order_processing_seconds");
        Console.WriteLine("Prometheus endpoint configured in appsettings: /metrics");

        activeWorkersGauge.Set(0, "orders-worker");
        applicationLifetime.StopApplication();
    }
}

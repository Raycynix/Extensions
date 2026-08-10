using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Tracing;
using Raycynix.Extensions.Tracing.Abstractions;

using var activityListener = new ActivityListener
{
    ShouldListenTo = static _ => true,
    Sample = static (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
    SampleUsingParentId = static (ref ActivityCreationOptions<string> _) => ActivitySamplingResult.AllDataAndRecorded
};

ActivitySource.AddActivityListener(activityListener);

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRaycynixTracing();
builder.Services.AddHostedService<TracingExampleWorker>();

await builder.Build().RunAsync();

internal sealed class TracingExampleWorker(
    IHostApplicationLifetime applicationLifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Raycynix tracing example");

        using (var activity = RaycynixTracing.ActivitySource.StartActivity("orders.process", ActivityKind.Internal))
        {
            activity?.SetTag("order.id", "ORD-2026-0001");
            activity?.SetTag("operation.type", "command");
            activity?.SetBaggage("tenant", "alpha");
            activity?.SetTag("customer.id", "customer-42");

            Console.WriteLine($"Root TraceId: {Activity.Current?.TraceId}");
            Console.WriteLine($"Root SpanId: {Activity.Current?.SpanId}");
            Console.WriteLine($"Tenant baggage: {Activity.Current?.GetBaggageItem("tenant")}");

            using (var child = RaycynixTracing.ActivitySource.StartActivity(
                       "orders.reserve_inventory",
                       ActivityKind.Internal))
            {
                child?.SetTag("sku", "SKU-RED-MUG");
                child?.SetTag("inventory.location", "warehouse-a");

                Console.WriteLine($"Child TraceId: {Activity.Current?.TraceId}");
                Console.WriteLine($"Child SpanId: {Activity.Current?.SpanId}");
                Console.WriteLine($"Inherited baggage: {Activity.Current?.GetBaggageItem("tenant")}");

                await Task.Delay(50, stoppingToken);
            }
        }

        applicationLifetime.StopApplication();
    }
}

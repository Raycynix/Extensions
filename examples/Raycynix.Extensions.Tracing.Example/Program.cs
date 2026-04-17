using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Tracing;
using Raycynix.Extensions.Tracing.Abstractions.Interfaces;

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
    ITracer tracer,
    IHostApplicationLifetime applicationLifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Raycynix tracing example");

        using (tracer.StartTrace("orders.process", new Dictionary<string, string>
               {
                   ["order.id"] = "ORD-2026-0001",
                   ["operation.type"] = "command"
               }))
        {
            tracer.SetBaggage("tenant", "alpha");
            tracer.AddTag("customer.id", "customer-42");

            Console.WriteLine($"Root TraceId: {Activity.Current?.TraceId}");
            Console.WriteLine($"Root SpanId: {Activity.Current?.SpanId}");
            Console.WriteLine($"Tenant baggage: {tracer.GetBaggage("tenant")}");

            using (tracer.StartTrace("orders.reserve_inventory", new Dictionary<string, string>
                   {
                       ["sku"] = "SKU-RED-MUG"
                   }))
            {
                tracer.AddTag("inventory.location", "warehouse-a");

                Console.WriteLine($"Child TraceId: {Activity.Current?.TraceId}");
                Console.WriteLine($"Child SpanId: {Activity.Current?.SpanId}");
                Console.WriteLine($"Inherited baggage: {tracer.GetBaggage("tenant")}");

                await Task.Delay(50, stoppingToken);
            }
        }

        applicationLifetime.StopApplication();
    }
}

using System.Diagnostics.Metrics;
using Raycynix.Extensions.Metrics.Abstractions;
using Raycynix.Extensions.Metrics.AspNetCore;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRaycynixAspNetCoreMetrics(metrics => metrics.AddPrometheusExporter());

var app = builder.Build();
var meter = RaycynixMetrics.CreateMeter(app.Services.GetRequiredService<IMeterFactory>());
var requestCounter = meter.CreateCounter<long>(
    "raycynix.example.requests",
    unit: "{request}",
    description: "Requests served by sample endpoints.");
var inventoryChanges = meter.CreateUpDownCounter<long>(
    "raycynix.example.inventory.changes",
    unit: "{item}",
    description: "Changes to the example inventory.");
var checkoutDuration = meter.CreateHistogram<double>(
    "raycynix.example.checkout.duration",
    unit: "s",
    description: "Checkout duration.");

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.MapGet("/", () => Results.Ok(new
{
    Service = "Raycynix.Extensions.Metrics.AspNetCore.Example",
    Endpoints = new[] { "GET /orders/process", "POST /inventory/{sku}/reserve", "GET /metrics" }
}));

app.MapGet("/orders/process", async (CancellationToken cancellationToken) =>
{
    requestCounter.Add(1, new KeyValuePair<string, object?>("raycynix.endpoint", "orders_process"));

    using (checkoutDuration.MeasureDuration(
               new KeyValuePair<string, object?>("raycynix.result", "success")))
    {
        await Task.Delay(120, cancellationToken);
    }

    return Results.Ok(new { Message = "Order processed and metrics updated." });
});

app.MapPost("/inventory/{sku}/reserve", (string sku) =>
{
    requestCounter.Add(1, new KeyValuePair<string, object?>("raycynix.endpoint", "inventory_reserve"));
    inventoryChanges.Add(-1, new KeyValuePair<string, object?>("raycynix.inventory.sku", sku));

    return Results.Accepted($"/inventory/{sku}", new { Message = "Inventory reserved.", Sku = sku });
});

app.Run();

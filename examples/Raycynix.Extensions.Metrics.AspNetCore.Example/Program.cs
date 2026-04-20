using Raycynix.Extensions.Metrics;
using Raycynix.Extensions.Metrics.Abstractions.Interfaces;
using Raycynix.Extensions.Metrics.AspNetCore;

Environment.CurrentDirectory = AppContext.BaseDirectory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRaycynixMetrics(builder.Configuration, options =>
{
    options.MetricsEndpoint = "/metrics";
    options.UsePrometheus = true;
});

var app = builder.Build();

app.UseRaycynixMetrics();

var requestCounter = app.Services.GetRequiredService<IMetricsService>()
    .CreateCounter("raycynix_http_requests_total", "HTTP requests served by sample endpoints.", "endpoint");
var inventoryGauge = app.Services.GetRequiredService<IMetricsService>()
    .CreateGauge("raycynix_inventory_items", "Current inventory amount.", "sku");
var checkoutHistogram = app.Services.GetRequiredService<IMetricsService>()
    .CreateHistogram("raycynix_checkout_duration_seconds", "Checkout duration in seconds.", "result");

inventoryGauge.Set(10, "SKU-RED-MUG");

app.MapGet("/", () => Results.Ok(new
{
    Service = "Raycynix.Extensions.Metrics.AspNetCore.Example",
    Endpoints = new[]
    {
        "GET /orders/process",
        "POST /inventory/{sku}/reserve",
        "GET /healthz",
        "GET /metrics"
    }
}));

app.MapGet("/orders/process", async (CancellationToken cancellationToken) =>
{
    requestCounter.Increment(labelValues: ["orders_process"]);

    using (checkoutHistogram.MeasureDuration("success"))
    {
        await Task.Delay(120, cancellationToken);
    }

    return Results.Ok(new
    {
        Message = "Order processed and metrics updated."
    });
});

app.MapPost("/inventory/{sku}/reserve", (string sku) =>
{
    requestCounter.Increment(labelValues: ["inventory_reserve"]);
    inventoryGauge.Decrement(1, sku);

    return Results.Accepted($"/inventory/{sku}", new
    {
        Message = "Inventory reserved.",
        Sku = sku
    });
});

app.MapGet("/healthz", () =>
{
    requestCounter.Increment(labelValues: ["healthz"]);

    return Results.Ok(new
    {
        Status = "Healthy"
    });
});

app.MapRaycynixMetrics("/metrics");

app.Run();

using System.Diagnostics;
using Raycynix.Extensions.Tracing;
using Raycynix.Extensions.Tracing.Abstractions.Interfaces;
using Raycynix.Extensions.Tracing.AspNetCore;

using var activityListener = new ActivityListener();
activityListener.ShouldListenTo = static _ => true;
activityListener.Sample = static (ref _) => ActivitySamplingResult.AllDataAndRecorded;
activityListener.SampleUsingParentId = static (ref _) => ActivitySamplingResult.AllDataAndRecorded;

ActivitySource.AddActivityListener(activityListener);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRaycynixTracing();

var app = builder.Build();

app.UseRaycynixTracing();

app.MapGet("/", () => Results.Ok(new
{
    Service = "Raycynix.Extensions.Tracing.AspNetCore.Example",
    Endpoints = new[]
    {
        "GET /trace",
        "GET /orders/{id}",
        "GET /inventory/{sku}"
    }
}));

app.MapGet("/trace", (HttpContext httpContext) => Results.Ok(new
{
    TraceId = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier,
    SpanId = Activity.Current?.SpanId.ToString()
}));

app.MapGet("/orders/{id}", async (string id, ITracer tracer, HttpContext httpContext, CancellationToken cancellationToken) =>
{
    using (tracer.StartTrace("orders.get", new Dictionary<string, string>
           {
               ["order.id"] = id
           }))
    {
        tracer.SetBaggage("tenant", "alpha");
        tracer.AddTag("http.route", "/orders/{id}");

        using (tracer.StartTrace("orders.load_payment"))
        {
            tracer.AddTag("payment.provider", "demo-gateway");
            await Task.Delay(40, cancellationToken);
        }

        return Results.Ok(new
        {
            OrderId = id,
            TraceId = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier,
            SpanId = Activity.Current?.SpanId.ToString(),
            Tenant = tracer.GetBaggage("tenant")
        });
    }
});

app.MapGet("/inventory/{sku}", (string sku, ITracer tracer, HttpContext httpContext) =>
{
    using (tracer.StartTrace("inventory.get", new Dictionary<string, string>
           {
               ["sku"] = sku
           }))
    {
        tracer.AddTag("inventory.source", "cache");

        return Results.Ok(new
        {
            Sku = sku,
            Available = 12,
            TraceId = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier,
            SpanId = Activity.Current?.SpanId.ToString()
        });
    }
});

app.Run();

using System.Diagnostics;
using Raycynix.Extensions.Tracing.Abstractions;
using Raycynix.Extensions.Tracing.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRaycynixAspNetCoreTracing();

var app = builder.Build();

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

app.MapGet("/orders/{id}", async (string id, HttpContext httpContext, CancellationToken cancellationToken) =>
{
    using (var activity = RaycynixTracing.ActivitySource.StartActivity("orders.get", ActivityKind.Internal))
    {
        activity?.SetTag("order.id", id);
        activity?.SetBaggage("tenant", "alpha");
        activity?.SetTag("http.route", "/orders/{id}");

        using (var payment = RaycynixTracing.ActivitySource.StartActivity("orders.load_payment"))
        {
            payment?.SetTag("payment.provider", "demo-gateway");
            await Task.Delay(40, cancellationToken);
        }

        return Results.Ok(new
        {
            OrderId = id,
            TraceId = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier,
            SpanId = Activity.Current?.SpanId.ToString(),
            Tenant = Activity.Current?.GetBaggageItem("tenant")
        });
    }
});

app.MapGet("/inventory/{sku}", (string sku, HttpContext httpContext) =>
{
    using (var activity = RaycynixTracing.ActivitySource.StartActivity("inventory.get", ActivityKind.Internal))
    {
        activity?.SetTag("sku", sku);
        activity?.SetTag("inventory.source", "cache");

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

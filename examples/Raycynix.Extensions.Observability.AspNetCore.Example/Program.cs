using System.Diagnostics;
using System.Net;
using Raycynix.Extensions.Common.Context;
using System.Diagnostics.Metrics;
using Raycynix.Extensions.Metrics.Abstractions;
using Raycynix.Extensions.Metrics.AspNetCore;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using Raycynix.Extensions.Observability.AspNetCore;
using Raycynix.Extensions.Observability.AspNetCore.Middleware;
using Raycynix.Extensions.Tracing.Abstractions.Interfaces;

Environment.CurrentDirectory = AppContext.BaseDirectory;

using var activityListener = new ActivityListener();
activityListener.ShouldListenTo = static _ => true;
activityListener.Sample = static (ref _) => ActivitySamplingResult.AllDataAndRecorded;
activityListener.SampleUsingParentId = static (ref _) => ActivitySamplingResult.AllDataAndRecorded;

ActivitySource.AddActivityListener(activityListener);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRaycynixAspNetCoreObservability();
builder.Services.AddOpenTelemetry().WithMetrics(metrics => metrics.AddPrometheusExporter());
builder.Services.AddHttpClient("downstream")
    .ConfigurePrimaryHttpMessageHandler(() => new EchoCorrelationHandler());

var app = builder.Build();

app.UseRaycynixObservability();
app.UseOpenTelemetryPrometheusScrapingEndpoint();

var meter = RaycynixMetrics.CreateMeter(app.Services.GetRequiredService<IMeterFactory>());
var requestCounter = meter.CreateCounter<long>("raycynix.observability.http.requests", "{request}");
var activeRequests = meter.CreateUpDownCounter<long>("raycynix.observability.http.active_requests", "{request}");
var requestDurationHistogram = meter.CreateHistogram<double>("raycynix.observability.http.request.duration", "s");

app.MapGet("/", () => Results.Ok(new
{
    Service = "Raycynix.Extensions.Observability.AspNetCore.Example",
    Endpoints = new[]
    {
        "GET /context",
        "GET /checkout",
        "GET /outbound",
        "GET /health",
        "GET /metrics"
    }
}));

app.MapGet("/context", (
    HttpContext httpContext,
    IOperationContext operationContext,
    ITracer tracer,
    ILoggerFactory loggerFactory) =>
{
    activeRequests.Add(1, new KeyValuePair<string, object?>("raycynix.endpoint", "context"));

    try
    {
        using (requestDurationHistogram.MeasureDuration(
                   new KeyValuePair<string, object?>("raycynix.endpoint", "context")))
        using (tracer.StartTrace("observability.context"))
        {
            requestCounter.Add(1, new KeyValuePair<string, object?>("raycynix.endpoint", "context"));
            var logger = loggerFactory.CreateLogger("ContextEndpoint");
            logger.LogInformation("Returning current observability context.");

            return Results.Ok(new
            {
                operationContext.CorrelationId,
                OperationTraceId = operationContext.TraceId,
                operationContext.UserId,
                TraceId = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier,
                SpanId = Activity.Current?.SpanId.ToString()
            });
        }
    }
    finally
    {
        activeRequests.Add(-1, new KeyValuePair<string, object?>("raycynix.endpoint", "context"));
    }
});

app.MapGet("/checkout", async (
    IOperationContext operationContext,
    ILogger<CheckoutEndpoint> logger,
    ITracer tracer,
    CancellationToken cancellationToken) =>
{
    activeRequests.Add(1, new KeyValuePair<string, object?>("raycynix.endpoint", "checkout"));

    try
    {
        using (requestDurationHistogram.MeasureDuration(
                   new KeyValuePair<string, object?>("raycynix.endpoint", "checkout")))
        using (tracer.StartTrace("checkout.handle", new Dictionary<string, string>
               {
                   ["feature"] = "observability"
               }))
        {
            requestCounter.Add(1, new KeyValuePair<string, object?>("raycynix.endpoint", "checkout"));
            tracer.AddTag("correlation.id", operationContext.CorrelationId);

            logger.LogInformation(
                "Handling checkout request. CorrelationId:{CorrelationId} TraceId:{TraceId}",
                operationContext.CorrelationId,
                operationContext.TraceId);

            await Task.Delay(40, cancellationToken);

            return Results.Ok(new
            {
                Message = "Checkout handled.",
                operationContext.CorrelationId,
                TraceId = Activity.Current?.TraceId.ToString(),
                SpanId = Activity.Current?.SpanId.ToString()
            });
        }
    }
    finally
    {
        activeRequests.Add(-1, new KeyValuePair<string, object?>("raycynix.endpoint", "checkout"));
    }
});

app.MapGet("/outbound", async (
    IHttpClientFactory httpClientFactory,
    CancellationToken cancellationToken) =>
{
    activeRequests.Add(1, new KeyValuePair<string, object?>("raycynix.endpoint", "outbound"));

    try
    {
        using (requestDurationHistogram.MeasureDuration(
                   new KeyValuePair<string, object?>("raycynix.endpoint", "outbound")))
        {
            requestCounter.Add(1, new KeyValuePair<string, object?>("raycynix.endpoint", "outbound"));

            var client = httpClientFactory.CreateClient("downstream");
            var response = await client.GetAsync("https://example.test/downstream", cancellationToken);
            var propagatedCorrelationId = await response.Content.ReadAsStringAsync(cancellationToken);

            return Results.Ok(new
            {
                PropagatedCorrelationId = propagatedCorrelationId
            });
        }
    }
    finally
    {
        activeRequests.Add(-1, new KeyValuePair<string, object?>("raycynix.endpoint", "outbound"));
    }
});

app.MapRaycynixObservabilityEndpoints();

app.Run();

internal sealed class CheckoutEndpoint;

internal sealed class EchoCorrelationHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.TryGetValues("X-Correlation-ID", out var values);
        var correlationId = values?.SingleOrDefault() ?? string.Empty;

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(correlationId)
        });
    }
}

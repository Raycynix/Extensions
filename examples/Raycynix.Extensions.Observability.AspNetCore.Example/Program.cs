using System.Diagnostics;
using System.Net;
using Raycynix.Extensions.Common.Context;
using Raycynix.Extensions.Metrics.Abstractions.Interfaces;
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
builder.Services.AddHttpClient("downstream")
    .ConfigurePrimaryHttpMessageHandler(() => new EchoCorrelationHandler());

var app = builder.Build();

app.UseRaycynixObservability();

var metrics = app.Services.GetRequiredService<IMetricsService>();
var requestCounter = metrics.CreateCounter(
    "raycynix_observability_http_requests_total",
    "Requests handled by observability example.",
    "endpoint");
var activeRequestsGauge = metrics.CreateGauge(
    "raycynix_observability_active_requests",
    "Current active requests by endpoint.",
    "endpoint");
var requestDurationHistogram = metrics.CreateHistogram(
    "raycynix_observability_request_duration_seconds",
    "Request duration for observability example.",
    "endpoint");

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
    activeRequestsGauge.Increment(labelValues: ["context"]);

    try
    {
        using (requestDurationHistogram.MeasureDuration("context"))
        using (tracer.StartTrace("observability.context"))
        {
            requestCounter.Increment(labelValues: ["context"]);
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
        activeRequestsGauge.Decrement(labelValues: ["context"]);
    }
});

app.MapGet("/checkout", async (
    IOperationContext operationContext,
    ILogger<CheckoutEndpoint> logger,
    ITracer tracer,
    CancellationToken cancellationToken) =>
{
    activeRequestsGauge.Increment(labelValues: ["checkout"]);

    try
    {
        using (requestDurationHistogram.MeasureDuration("checkout"))
        using (tracer.StartTrace("checkout.handle", new Dictionary<string, string>
               {
                   ["feature"] = "observability"
               }))
        {
            requestCounter.Increment(labelValues: ["checkout"]);
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
        activeRequestsGauge.Decrement(labelValues: ["checkout"]);
    }
});

app.MapGet("/outbound", async (
    IHttpClientFactory httpClientFactory,
    CancellationToken cancellationToken) =>
{
    activeRequestsGauge.Increment(labelValues: ["outbound"]);

    try
    {
        using (requestDurationHistogram.MeasureDuration("outbound"))
        {
            requestCounter.Increment(labelValues: ["outbound"]);

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
        activeRequestsGauge.Decrement(labelValues: ["outbound"]);
    }
});

app.MapRaycynixObservabilityEndpoints("/health", "/metrics");

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
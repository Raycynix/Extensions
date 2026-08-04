using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Common.Context;
using Raycynix.Extensions.Metrics.Abstractions;
using Raycynix.Extensions.Tracing.Abstractions;

namespace Raycynix.Extensions.Observability.Example;

internal sealed class ObservabilityExampleWorker(
    IOperationContext operationContext,
    ILogger<ObservabilityExampleWorker> logger,
    IMeterFactory meterFactory,
    IHostApplicationLifetime applicationLifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        operationContext.CorrelationId = "obs-example-correlation";
        operationContext.UserId = "demo-user";

        var meter = RaycynixMetrics.CreateMeter(meterFactory);
        var requestCounter = meter.CreateCounter<long>(
            "raycynix.observability.example.requests", "{request}");
        var queueDepth = meter.CreateUpDownCounter<long>(
            "raycynix.observability.example.queue.depth", "{item}");
        var durationHistogram = meter.CreateHistogram<double>(
            "raycynix.observability.example.duration", "s");

        using (var activity = RaycynixTracing.ActivitySource.StartActivity("checkout.handle", ActivityKind.Internal))
        {
            activity?.SetTag("tenant", "alpha");
            activity?.SetTag("operation.kind", "command");
            activity?.SetBaggage("tenant", "alpha");
            activity?.SetTag("correlation.id", operationContext.CorrelationId);

            queueDepth.Add(3, new KeyValuePair<string, object?>("raycynix.queue", "checkout"));

            using (durationHistogram.MeasureDuration(
                       new KeyValuePair<string, object?>("raycynix.operation", "checkout.handle")))
            {
                logger.LogInformation("Handling checkout request correlationId:{CorrelationId}",
                    operationContext.CorrelationId);

                await Task.Delay(80, stoppingToken);
                requestCounter.Add(
                    1,
                    new("raycynix.operation", "checkout.handle"),
                    new("raycynix.status", "success"));
            }

            using (var publish = RaycynixTracing.ActivitySource.StartActivity("checkout.publish_event"))
            {
                publish?.SetTag("message.destination", "orders.events");

                using (durationHistogram.MeasureDuration(
                           new KeyValuePair<string, object?>("raycynix.operation", "checkout.publish_event")))
                {
                    await Task.Delay(30, stoppingToken);
                    requestCounter.Add(
                        1,
                        new("raycynix.operation", "checkout.publish_event"),
                        new("raycynix.status", "success"));
                }
            }
        }

        queueDepth.Add(-3, new KeyValuePair<string, object?>("raycynix.queue", "checkout"));
        logger.LogInformation("Observability example finished");

        applicationLifetime.StopApplication();
    }
}

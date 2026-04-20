using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Common.Context;
using Raycynix.Extensions.Logging.Abstractions;
using Raycynix.Extensions.Metrics.Abstractions.Interfaces;
using Raycynix.Extensions.Tracing.Abstractions.Interfaces;

namespace Raycynix.Extensions.Observability.Example;

internal sealed class ObservabilityExampleWorker(
    IOperationContext operationContext,
    ILogger<ObservabilityExampleWorker> logger,
    ITracer tracer,
    IMetricsService metricsService,
    IHostApplicationLifetime applicationLifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        operationContext.CorrelationId = "obs-example-correlation";
        operationContext.UserId = "demo-user";

        var requestCounter = metricsService.CreateCounter(
            "raycynix_observability_example_requests_total",
            "Number of synthetic operations executed.",
            "operation",
            "status");

        var queueGauge = metricsService.CreateGauge(
            "raycynix_observability_example_queue_depth",
            "Current background queue depth.",
            "queue");

        var durationHistogram = metricsService.CreateHistogram(
            "raycynix_observability_example_duration_seconds",
            "Duration of synthetic operations.",
            "operation");

        using (tracer.StartTrace("checkout.handle", new Dictionary<string, string>
               {
                   ["tenant"] = "alpha",
                   ["operation.kind"] = "command"
               }))
        {
            tracer.SetBaggage("tenant", "alpha");
            tracer.AddTag("correlation.id", operationContext.CorrelationId);

            queueGauge.Set(3, "checkout");

            using (durationHistogram.MeasureDuration("checkout.handle"))
            {
                logger.Information("Handling checkout request correlationId:{CorrelationId}",
                    operationContext.CorrelationId);

                await Task.Delay(80, stoppingToken);
                requestCounter.Increment(labelValues: ["checkout.handle", "success"]);
            }

            using (tracer.StartTrace("checkout.publish_event"))
            {
                tracer.AddTag("message.destination", "orders.events");

                using (durationHistogram.MeasureDuration("checkout.publish_event"))
                {
                    await Task.Delay(30, stoppingToken);
                    requestCounter.Increment(labelValues: ["checkout.publish_event", "success"]);
                }
            }
        }

        queueGauge.Set(0, "checkout");
        logger.Information("Observability example finished");

        applicationLifetime.StopApplication();
    }
}
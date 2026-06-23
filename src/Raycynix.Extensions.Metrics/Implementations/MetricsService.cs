using Microsoft.Extensions.Logging;
using Prometheus;
using Raycynix.Extensions.Metrics.Abstractions.Interfaces;

namespace Raycynix.Extensions.Metrics.Implementations;

/// <summary>
/// Creates Prometheus-backed counters, gauges, and histograms for the Raycynix metrics abstractions.
/// </summary>
internal class MetricsService(ILogger<MetricsService>? logger = null) : IMetricsService
{
    private readonly MetricFactory _metricFactory =
        Prometheus.Metrics.WithCustomRegistry(Prometheus.Metrics.DefaultRegistry);

    public IMetricCounter CreateCounter(string name, string help, params string[] labelNames)
    {
        logger?.LogDebug(
            "Creating Prometheus counter. MetricName:{MetricName} LabelCount:{LabelCount}",
            name,
            labelNames.Length);

        var counter = _metricFactory.CreateCounter(name, help, labelNames);
        return new MetricCounter(counter);
    }

    public IMetricGauge CreateGauge(string name, string help, params string[] labelNames)
    {
        logger?.LogDebug(
            "Creating Prometheus gauge. MetricName:{MetricName} LabelCount:{LabelCount}",
            name,
            labelNames.Length);

        var gauge = _metricFactory.CreateGauge(name, help, labelNames);
        return new MetricsGauge(gauge);
    }

    public IMetricHistogram CreateHistogram(string name, string help, params string[] labelNames)
    {
        logger?.LogDebug(
            "Creating Prometheus histogram. MetricName:{MetricName} LabelCount:{LabelCount}",
            name,
            labelNames.Length);

        var histogram = _metricFactory.CreateHistogram(name, help, new HistogramConfiguration
        {
            LabelNames = labelNames,
            Buckets = [0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10]
        });

        return new MetricsHistogram(histogram);
    }
}
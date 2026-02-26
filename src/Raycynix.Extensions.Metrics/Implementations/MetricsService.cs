using Prometheus;
using Raycynix.Extensions.Metrics.Abstractions;

namespace Raycynix.Extensions.Metrics.Implementations;

internal class MetricsService : IMetricsService
{
    private readonly MetricFactory _metricFactory =
        Prometheus.Metrics.WithCustomRegistry(Prometheus.Metrics.DefaultRegistry);

    public IMetricCounter CreateCounter(string name, string help, params string[] labelNames)
    {
        var counter = _metricFactory.CreateCounter(name, help, labelNames);
        return new MetricCounter(counter);
    }

    public IMetricGauge CreateGauge(string name, string help, params string[] labelNames)
    {
        var gauge = _metricFactory.CreateGauge(name, help, labelNames);
        return new MetricsGauge(gauge);
    }

    public IMetricHistogram CreateHistogram(string name, string help, params string[] labelNames)
    {var histogram = _metricFactory.CreateHistogram(name, help, new HistogramConfiguration
        {
            LabelNames = labelNames,
            // Стандартные бакеты для Latency (в секундах)
            Buckets = [0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10]
        });
        return new MetricsHistogram(histogram);
    }
}
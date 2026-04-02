using Prometheus;
using Raycynix.Extensions.Metrics.Abstractions;
using Raycynix.Extensions.Metrics.Abstractions.Interfaces;

namespace Raycynix.Extensions.Metrics.Implementations;

/// <inheritdoc />
internal class MetricsHistogram(Histogram histogram) : IMetricHistogram
{
    public void Observe(double value, params string[] labelValues) => histogram.WithLabels(labelValues).Observe(value);

    public IDisposable MeasureDuration(params string[] labelValues) => histogram.WithLabels(labelValues).NewTimer();
}
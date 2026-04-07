using Prometheus;
using Raycynix.Extensions.Metrics.Abstractions.Interfaces;

namespace Raycynix.Extensions.Metrics.Implementations;

/// <summary>
/// Adapts a Prometheus histogram to the Raycynix histogram abstraction.
/// </summary>
internal class MetricsHistogram(Histogram histogram) : IMetricHistogram
{
    /// <inheritdoc />
    public void Observe(double value, params string[] labelValues) => histogram.WithLabels(labelValues).Observe(value);

    /// <inheritdoc />
    public IDisposable MeasureDuration(params string[] labelValues) => histogram.WithLabels(labelValues).NewTimer();
}

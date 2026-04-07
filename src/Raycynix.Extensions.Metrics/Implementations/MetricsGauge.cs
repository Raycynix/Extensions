using Prometheus;
using Raycynix.Extensions.Metrics.Abstractions.Interfaces;

namespace Raycynix.Extensions.Metrics.Implementations;

/// <summary>
/// Adapts a Prometheus gauge to the Raycynix gauge abstraction.
/// </summary>
public class MetricsGauge(Gauge gauge) : IMetricGauge
{
    /// <inheritdoc />
    public void Set(double value, params string[] labelValues) => gauge.WithLabels(labelValues).Set(value);

    /// <inheritdoc />
    public void Increment(double value = 1, params string[] labelValues) => gauge.WithLabels(labelValues).Inc(value);

    /// <inheritdoc />
    public void Decrement(double value = 1, params string[] labelValues) =>  gauge.WithLabels(labelValues).Dec(value);
}

using Prometheus;
using Raycynix.Extensions.Metrics.Abstractions.Interfaces;

namespace Raycynix.Extensions.Metrics.Implementations;

/// <summary>
/// Adapts a Prometheus counter to the Raycynix counter abstraction.
/// </summary>
internal class MetricCounter(Counter counter) : IMetricCounter
{
    /// <inheritdoc />
    public void Increment(double value = 1, params string[] labelValues) => counter.WithLabels(labelValues).Inc(value); 
}

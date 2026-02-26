using Prometheus;
using Raycynix.Extensions.Metrics.Abstractions;

namespace Raycynix.Extensions.Metrics.Implementations;

internal class MetricCounter(Counter counter) : IMetricCounter
{
    public void Increment(double value = 1, params string[] labelValues) => counter.WithLabels(labelValues).Inc(value); 
}
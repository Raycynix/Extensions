using Prometheus;
using Raycynix.Extensions.Metrics.Abstractions;

namespace Raycynix.Extensions.Metrics.Implementations;

public class MetricsGauge(Gauge gauge) : IMetricGauge
{
    
    public void Set(double value, params string[] labelValues) => gauge.WithLabels(labelValues).Set(value);
    
    public void Increment(double value = 1, params string[] labelValues) => gauge.WithLabels(labelValues).Inc(value);

    public void Decrement(double value = 1, params string[] labelValues) =>  gauge.WithLabels(labelValues).Dec(value);
}
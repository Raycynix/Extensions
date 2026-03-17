namespace Raycynix.Extensions.Metrics.Abstractions;

/// <summary>
/// Represents a counter metric.
/// </summary>
public interface IMetricCounter
{
    /// <summary>
    /// Increments the counter by the specified value.
    /// </summary>
    /// <param name="value">The increment value.</param>
    /// <param name="labelValues">The label values for the metric instance.</param>
    void Increment(double value = 1, params string[] labelValues);
}

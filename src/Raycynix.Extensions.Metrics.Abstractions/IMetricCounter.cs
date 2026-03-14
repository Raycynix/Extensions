namespace Raycynix.Extensions.Metrics.Abstractions;

/// <summary>
/// Represents an abstraction for a metric counter, which is used to accumulate counts
/// for a specific metric over time, with optional support for labeled dimensions.
/// </summary>
public interface IMetricCounter
{
    /// <summary>
    /// Increments the metric counter by the specified value, optionally associating
    /// it with a set of labeled dimensions.
    /// </summary>
    /// <param name="value">The amount by which to increment the counter. Defaults to 1.</param>
    /// <param name="labelValues">
    /// An optional array of label values to associate with the increment. These should
    /// correspond to the label names defined for the metric.
    /// </param>
    void Increment(double value = 1, params string[] labelValues);
}
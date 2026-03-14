namespace Raycynix.Extensions.Metrics.Abstractions;

/// <summary>
/// Defines the contract for a gauge metric, which represents a single numeric value that can
/// be adjusted dynamically. Gauges are typically used to measure values that vary over time,
/// such as resource consumption or physical quantities.
/// </summary>
public interface IMetricGauge
{
    /// <summary>
    /// Sets the value of the gauge metric to the specified value. If labels are provided,
    /// the value will be set for the specific label combination.
    /// </summary>
    /// <param name="value">
    /// The numeric value to set the gauge metric to.
    /// </param>
    /// <param name="labelValues">
    /// An optional array of label values corresponding to the labeled dimensions of the gauge metric.
    /// These labels allow the metric to differentiate between different conceptual instances.
    /// </param>
    void Set(double value, params string[] labelValues);

    /// <summary>
    /// Increments the value of the gauge metric by the specified amount. If label values are provided,
    /// the increment is applied to the specific label combination.
    /// </summary>
    /// <param name="value">
    /// The numeric amount by which to increment the gauge metric. Defaults to 1 if not specified.
    /// </param>
    /// <param name="labelValues">
    /// An optional array of label values corresponding to the labeled dimensions of the gauge metric.
    /// These labels allow the metric to differentiate between different conceptual instances.
    /// </param>
    void Increment(double value = 1, params string[] labelValues);

    /// <summary>
    /// Decrements the value of the gauge metric by the specified amount. If label values are provided,
    /// the decrement is applied to the specific label combination.
    /// </summary>
    /// <param name="value">
    /// The numeric amount by which to decrement the gauge metric. Defaults to 1 if not specified.
    /// </param>
    /// <param name="labelValues">
    /// An optional array of label values corresponding to the labeled dimensions of the gauge metric.
    /// These labels allow the metric to differentiate between different conceptual instances.
    /// </param>
    void Decrement(double value = 1, params string[] labelValues);
}
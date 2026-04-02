namespace Raycynix.Extensions.Metrics.Abstractions.Interfaces;

/// <summary>
/// Represents a gauge metric.
/// </summary>
public interface IMetricGauge
{
    /// <summary>
    /// Sets the gauge value.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <param name="labelValues">The label values for the metric instance.</param>
    void Set(double value, params string[] labelValues);

    /// <summary>
    /// Increments the gauge value.
    /// </summary>
    /// <param name="value">The increment value.</param>
    /// <param name="labelValues">The label values for the metric instance.</param>
    void Increment(double value = 1, params string[] labelValues);

    /// <summary>
    /// Decrements the gauge value.
    /// </summary>
    /// <param name="value">The decrement value.</param>
    /// <param name="labelValues">The label values for the metric instance.</param>
    void Decrement(double value = 1, params string[] labelValues);
}

namespace Raycynix.Extensions.Metrics.Abstractions.Interfaces;

/// <summary>
/// Represents a histogram metric.
/// </summary>
public interface IMetricHistogram
{
    /// <summary>
    /// Records a histogram observation.
    /// </summary>
    /// <param name="value">The value to record.</param>
    /// <param name="labelValues">The label values for the metric instance.</param>
    void Observe(double value, params string[] labelValues);

    /// <summary>
    /// Starts timing an operation and records the duration when disposed.
    /// </summary>
    /// <param name="labelValues">The label values for the metric instance.</param>
    /// <returns>A timer handle that records elapsed time on disposal.</returns>
    IDisposable MeasureDuration(params string[] labelValues);
}

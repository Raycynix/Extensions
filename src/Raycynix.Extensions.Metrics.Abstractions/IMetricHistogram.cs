namespace Raycynix.Extensions.Metrics.Abstractions;

/// <summary>
/// Provides methods for recording observations and measuring durations in a histogram metric.
/// </summary>
public interface IMetricHistogram
{
    /// <summary>
    /// Records an observation to the histogram with the specified value and optional label values.
    /// </summary>
    /// <param name="value">The value to be recorded in the histogram.</param>
    /// <param name="labelValues">Optional string array of label values that provide additional context for the observation.</param>
    void Observe(double value, params string[] labelValues);

    /// <summary>
    /// Measures the duration of an operation and records the observation to the histogram.
    /// </summary>
    /// <param name="labelValues">Optional string array of label values that provide additional context for the measurement.</param>
    /// <returns>An object that tracks the duration of the operation and records the time upon disposal.</returns>
    IDisposable MeasureDuration(params string[] labelValues);
}
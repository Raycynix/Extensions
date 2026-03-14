namespace Raycynix.Extensions.Metrics.Abstractions;

/// <summary>
/// Provides an abstraction for creating and managing various metric types, including counters, gauges, and histograms.
/// This service allows for the registration and manipulation of metrics with support for labeled dimensions.
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// Initializes and returns a new counter-metric with the specified parameters.
    /// </summary>
    /// <param name="name">The unique name assigned to the counter for identification purposes.</param>
    /// <param name="help">A detailed description that explains the purpose of the counter.</param>
    /// <param name="labelNames">A collection of labels to categorize and filter the counter-metrics.</param>
    /// <returns>An object implementing <see cref="IMetricCounter"/> that represents the created counter-metric.</returns>
    IMetricCounter CreateCounter(string name, string help, params string[] labelNames);

    /// <summary>
    /// Creates a new gauge metric with the specified name, description, and optional label names.
    /// Gauges are used to represent a single numerical value that can increase or decrease over time.
    /// </summary>
    /// <param name="name">The unique name assigned to the gauge, following the standard metric naming conventions.</param>
    /// <param name="help">A descriptive string that explains the purpose of the gauge metric.</param>
    /// <param name="labelNames">An optional array of label names used to provide additional dimensions for the gauge.</param>
    /// <returns>An instance of <see cref="IMetricGauge"/> that represents the created gauge metric, supporting value manipulation operations.</returns>
    IMetricGauge CreateGauge(string name, string help, params string[] labelNames);

    /// <summary>
    /// Initializes and returns a new histogram metric with the specified parameters.
    /// </summary>
    /// <param name="name">The unique name assigned to the histogram for identification purposes.</param>
    /// <param name="help">A detailed description that explains the purpose of the histogram.</param>
    /// <param name="labelNames">A collection of labels to categorize and filter the histogram metrics.</param>
    /// <returns>An object implementing <see cref="IMetricHistogram"/> that represents the created histogram metric.</returns>
    IMetricHistogram CreateHistogram(string name, string help, params string[] labelNames);
}
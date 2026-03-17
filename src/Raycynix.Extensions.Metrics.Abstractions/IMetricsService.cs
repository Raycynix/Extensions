namespace Raycynix.Extensions.Metrics.Abstractions;

/// <summary>
/// Creates counters, gauges, and histograms.
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// Creates a counter metric.
    /// </summary>
    /// <param name="name">The metric name.</param>
    /// <param name="help">The metric description.</param>
    /// <param name="labelNames">The metric label names.</param>
    /// <returns>The created counter.</returns>
    IMetricCounter CreateCounter(string name, string help, params string[] labelNames);

    /// <summary>
    /// Creates a gauge metric.
    /// </summary>
    /// <param name="name">The metric name.</param>
    /// <param name="help">The metric description.</param>
    /// <param name="labelNames">The metric label names.</param>
    /// <returns>The created gauge.</returns>
    IMetricGauge CreateGauge(string name, string help, params string[] labelNames);

    /// <summary>
    /// Creates a histogram metric.
    /// </summary>
    /// <param name="name">The metric name.</param>
    /// <param name="help">The metric description.</param>
    /// <param name="labelNames">The metric label names.</param>
    /// <returns>The created histogram.</returns>
    IMetricHistogram CreateHistogram(string name, string help, params string[] labelNames);
}

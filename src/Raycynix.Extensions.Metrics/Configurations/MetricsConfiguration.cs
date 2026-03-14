namespace Raycynix.Extensions.Metrics.Configurations;

/// <summary>
/// Represents the configuration settings for metrics, including optional integration with Prometheus and health checks.
/// </summary>
public class MetricsConfiguration
{
    /// <summary>
    /// Determines whether Prometheus metrics collection is enabled.
    /// Setting this property to true enables integration with Prometheus for gathering and exposing metrics data.
    /// </summary>
    public bool UsePrometheus { get; init; } = false;

    /// <summary>
    /// Specifies the endpoint where metrics are exposed for collection.
    /// This property defines the path at which metrics data will be available to external systems.
    /// </summary>
    public string MetricsEndpoint { get; init; } = "/metrics";

    /// <summary>
    /// Indicates whether health checks are enabled for the metrics configuration.
    /// When set to <c>true</c>, health checks will be accessible via the configured endpoints.
    /// </summary>
    public bool UseHealthChecks { get; init; } = true;
}
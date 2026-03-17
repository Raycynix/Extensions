namespace Raycynix.Extensions.Metrics.Configurations;

/// <summary>
/// Represents configuration settings for metrics exposure.
/// </summary>
public class MetricsConfiguration
{
    /// <summary>
    /// Gets a value indicating whether Prometheus integration is enabled.
    /// </summary>
    public bool UsePrometheus { get; init; } = false;

    /// <summary>
    /// Gets the endpoint path used to expose metrics.
    /// </summary>
    public string MetricsEndpoint { get; init; } = "/metrics";

    /// <summary>
    /// Gets a value indicating whether health checks are enabled.
    /// </summary>
    public bool UseHealthChecks { get; init; } = true;
}

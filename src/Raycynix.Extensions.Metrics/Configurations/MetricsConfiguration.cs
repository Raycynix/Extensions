namespace Raycynix.Extensions.Metrics.Configurations;

/// <summary>
/// Represents configuration settings for metrics exposure.
/// </summary>
public class MetricsConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether Prometheus integration is enabled.
    /// </summary>
    public bool UsePrometheus { get; set; } = true;

    /// <summary>
    /// Gets or sets the endpoint path used to expose metrics.
    /// </summary>
    public string MetricsEndpoint { get; set; } = "/metrics";

    /// <summary>
    /// Gets or sets a value indicating whether health checks are enabled.
    /// </summary>
    public bool UseHealthChecks { get; set; } = true;

    /// <summary>
    /// Validates the metrics configuration.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(MetricsEndpoint))
        {
            throw new InvalidOperationException("Metrics endpoint path cannot be empty.");
        }

        if (!MetricsEndpoint.StartsWith('/'))
        {
            throw new InvalidOperationException("Metrics endpoint path must start with '/'.");
        }
    }
}

namespace Raycynix.Extensions.Metrics.Configurations;

public class MetricsConfiguration
{
    public bool UsePrometheus { get; init; } = false;
    
    public string MetricsEndpoint { get; init; } = "/metrics";

    public bool UseHealthChecks { get; init; } = true;
}
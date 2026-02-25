using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Common.Helpers;

namespace Raycynix.Extensions.Logging.Configurations;

/// <summary>
/// Represents configuration options for the Raycynix logging module.
/// </summary>
public class LoggingConfiguration
{
    /// <summary>
    /// The name of the current service or application emitting logs.
    /// </summary>
    public string ServiceName { get; init; } = AssemblyHelper.CurrentName();

    /// <summary>
    /// The version of the current service or application emitting logs.
    /// </summary>
    public string ServiceVersion { get; init; } = AssemblyHelper.CurrentVersion();

    /// <summary>
    /// The environment name (e.g., Development, Production).
    /// </summary>
    public string Environment { get; init; } = EnvironmentHelper.CurrentEnvironment();

    /// <value>
    /// Indicating whether Elasticsearch logging is <b>enabled</b>
    /// </value>
    public bool UseElastic { get; init; } = false;

    /// <summary>
    /// The URI of the Elasticsearch server where logs are sent.
    /// </summary>
    public string ElasticUrl { get; init; } = "http://localhost:9200";

    /// <summary>
    /// The minimum log event level to capture.
    /// </summary>
    public LogLevel MinimumLevel { get; init; } = LogLevel.Information;

    /// <summary>
    /// The output log message
    /// </summary>
    public string OutputTemplate { get; init; } =
        "[{Timestamp:HH:mm:ss}] [{ServiceName}] [{ServiceVersion}] {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// 
    /// </summary>
    public bool UsePrometheus { get; init; } = false;
    
    /// <summary>
    /// 
    /// </summary>
    public string MetricsEndpoint { get; init; } = "/metrics";

    /// <summary>
    /// 
    /// </summary>
    public bool UseHealthChecks { get; init; } = true;

    //TODO: Create documentation
}
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Common.Helpers;

namespace Raycynix.Extensions.Logging.Configurations;

/// <summary>
/// Represents configuration settings for Raycynix logging.
/// </summary>
public class LoggingConfiguration
{
    /// <summary>
    /// Gets the service name written to log events.
    /// </summary>
    public string ServiceName { get; init; } = AssemblyHelper.CurrentName();

    /// <summary>
    /// Gets the service version written to log events.
    /// </summary>
    public string ServiceVersion { get; init; } = AssemblyHelper.CurrentVersion();

    /// <summary>
    /// Gets the current environment name.
    /// </summary>
    public string Environment { get; init; } = EnvironmentHelper.CurrentEnvironment();

    /// <summary>
    /// Gets a value indicating whether Elasticsearch logging is enabled.
    /// </summary>
    public bool UseElastic { get; init; } = false;

    /// <summary>
    /// Gets the Elasticsearch endpoint.
    /// </summary>
    public string ElasticUrl { get; init; } = "http://localhost:9200";

    /// <summary>
    /// Gets the minimum log level.
    /// </summary>
    public LogLevel MinimumLevel { get; init; } = LogLevel.Information;

    /// <summary>
    /// Gets the console output template.
    /// </summary>
    public string OutputTemplate { get; init; } =
        "[{Timestamp:HH:mm:ss}] [{Level:u3}] [{ServiceName}] [{ServiceVersion}] [Env:{Environment}] [Trace:{TraceId}] [Span:{SpanId}] [Corr:{CorrelationId}] {Message:lj}{NewLine}{Exception}";
}

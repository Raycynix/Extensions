using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Common.Helpers;

namespace Raycynix.Extensions.Logging.Abstractions.Configurations;

/// <summary>
/// Represents configuration settings for Raycynix logging.
/// </summary>
public class LoggingConfiguration
{
    /// <summary>
    /// Gets or sets the service name written to log events.
    /// </summary>
    public string ServiceName { get; set; } = AssemblyHelper.CurrentName();

    /// <summary>
    /// Gets or sets the service version written to log events.
    /// </summary>
    public string ServiceVersion { get; set; } = AssemblyHelper.CurrentVersion();

    /// <summary>
    /// Gets or sets the current environment name.
    /// </summary>
    public string Environment { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the minimum log level.
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Gets or sets the console output template.
    /// </summary>
    public string OutputTemplate { get; set; } =
        "[{Timestamp:HH:mm:ss}] [{Level:u3}] [{ServiceName}] [{ServiceVersion}] [Env:{Environment}] [Trace:{TraceId}] [Span:{SpanId}] [Corr:{CorrelationId}] {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// Validates the logging configuration.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ServiceName))
        {
            throw new InvalidOperationException("Logging service name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(ServiceVersion))
        {
            throw new InvalidOperationException("Logging service version cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(OutputTemplate))
        {
            throw new InvalidOperationException("Logging output template cannot be empty.");
        }
    }
}

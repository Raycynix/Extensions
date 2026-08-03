namespace Raycynix.Extensions.Serilog.Configurations;

/// <summary>
/// Contains Raycynix-specific conventions applied to the Serilog pipeline.
/// Native Serilog behavior such as sinks, filters, levels, and destructuring
/// should be configured through the standard Serilog configuration section.
/// </summary>
public sealed class RaycynixSerilogOptions
{
    /// <summary>
    /// Default configuration section containing Raycynix Serilog conventions.
    /// </summary>
    public const string SectionName = "Raycynix:Serilog";

    /// <summary>
    /// Gets or sets the service name added to every log event.
    /// When empty, the host application name is used.
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the service version added to every log event.
    /// When empty, the entry assembly informational version is used.
    /// </summary>
    public string ServiceVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the deployment environment added to every log event.
    /// When empty, the host environment name is used.
    /// </summary>
    public string Environment { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the section containing native Serilog configuration.
    /// </summary>
    public string SerilogSectionName { get; set; } = "Serilog";

    /// <summary>
    /// Gets or sets whether the package should apply its default
    /// Microsoft and System minimum-level overrides.
    /// Native Serilog configuration can override these values.
    /// </summary>
    public bool ApplyDefaultLevelOverrides { get; set; } = true;

    /// <summary>
    /// Gets or sets whether a console sink should be added when the native
    /// Serilog configuration does not contain any configured sinks.
    /// </summary>
    public bool UseDefaultConsoleWhenNoSinksConfigured { get; set; } = true;

    /// <summary>
    /// Gets or sets the output template used by the fallback console sink.
    /// </summary>
    public string DefaultConsoleOutputTemplate { get; set; } =
        "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] " +
        "[{ServiceName}] [{Environment}] [{SourceContext}] " +
        "[Trace:{TraceId}] [Span:{SpanId}] " +
        "{Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// Gets or sets whether the existing static Serilog logger should be preserved.
    /// </summary>
    public bool PreserveStaticLogger { get; set; }

    /// <summary>
    /// Gets or sets whether events should also be forwarded to registered
    /// Microsoft logging providers.
    /// </summary>
    public bool WriteToProviders { get; set; }
}
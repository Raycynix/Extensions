using Elastic.Ingest.Elasticsearch;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using Raycynix.Extensions.Serilog.Elastic.Enums;
using Serilog.Events;

namespace Raycynix.Extensions.Serilog.Elastic.Configurations;

/// <summary>
/// Contains Raycynix configuration for the official Elastic Serilog sink.
/// </summary>
public sealed class ElasticSerilogOptions
{
    /// <summary>
    /// Default configuration section for the Elastic integration.
    /// </summary>
    public const string SectionName = "Raycynix:Serilog:Elastic";

    /// <summary>
    /// Gets or sets whether the Elastic sink is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the Elastic connection mode.
    /// </summary>
    public ElasticConnectionMode ConnectionMode { get; set; } =
        ElasticConnectionMode.Elasticsearch;

    /// <summary>
    /// Gets the Elasticsearch node endpoints.
    /// At least one node is required in Elasticsearch connection mode.
    /// </summary>
    public IList<Uri> Nodes { get; set; } = new List<Uri>();

    /// <summary>
    /// Gets or sets whether Elasticsearch node sniffing is enabled.
    /// </summary>
    public bool UseSniffing { get; set; }

    /// <summary>
    /// Gets or sets the Elastic Cloud deployment ID.
    /// </summary>
    public string? CloudId { get; set; }

    /// <summary>
    /// Gets the authentication configuration.
    /// </summary>
    public ElasticAuthenticationOptions Authentication { get; set; } =
        new();

    /// <summary>
    /// Gets the data stream configuration.
    /// </summary>
    public ElasticDataStreamOptions DataStream { get; set; } =
        new();

    /// <summary>
    /// Gets the optional proxy configuration.
    /// </summary>
    public ElasticProxyOptions Proxy { get; set; } =
        new();

    /// <summary>
    /// Gets the optional channel buffer configuration.
    /// </summary>
    public ElasticBufferOptions Buffer { get; set; } =
        new();

    /// <summary>
    /// Gets or sets how the sink bootstraps Elasticsearch templates
    /// and data stream infrastructure.
    /// </summary>
    public BootstrapMethod BootstrapMethod { get; set; } =
        BootstrapMethod.Silent;

    /// <summary>
    /// Gets or sets the minimum level accepted by the Elastic sink.
    /// This does not change the global Serilog minimum level.
    /// </summary>
    public LogEventLevel MinimumLevel { get; set; } =
        LogEventLevel.Information;

    /// <summary>
    /// Gets or sets whether ECS host information is included.
    /// </summary>
    public bool IncludeHost { get; set; } = true;

    /// <summary>
    /// Gets or sets whether ECS process information is included.
    /// </summary>
    public bool IncludeProcess { get; set; } = true;

    /// <summary>
    /// Gets or sets whether ECS user information is included.
    /// </summary>
    public bool IncludeUser { get; set; }

    /// <summary>
    /// Gets or sets whether Activity trace and span data is included.
    /// </summary>
    public bool IncludeActivity { get; set; } = true;

    /// <summary>
    /// Gets Serilog properties that should not be copied into
    /// the resulting ECS document.
    /// </summary>
    public ISet<string> FilterProperties { get; set; } =
        new HashSet<string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the expected server certificate fingerprint.
    /// </summary>
    public string? CertificateFingerprint { get; set; }

    /// <summary>
    /// Gets or sets whether Elastic transport debug mode is enabled.
    /// </summary>
    public bool DebugMode { get; set; }

    /// <summary>
    /// Gets or sets the configurator execution order.
    /// </summary>
    public int Order { get; set; } = 1_000;

    /// <summary>
    /// Gets or sets an optional programmatic callback for modifying
    /// the official Elastic sink options.
    /// </summary>
    /// <remarks>
    /// This callback cannot be populated from configuration files.
    /// It executes after Raycynix defaults have been applied.
    /// </remarks>
    public Action<ElasticsearchSinkOptions>? ConfigureSinkOptions { get; set; }

    /// <summary>
    /// Gets or sets an optional programmatic callback for modifying
    /// the Elastic transport.
    /// </summary>
    /// <remarks>
    /// This callback cannot be populated from configuration files.
    /// It executes after Raycynix transport settings have been applied.
    /// </remarks>
    public Action<TransportConfigurationDescriptor>? ConfigureTransport { get; set; }
}
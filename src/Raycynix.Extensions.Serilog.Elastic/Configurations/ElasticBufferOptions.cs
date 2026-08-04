using System.Threading.Channels;

namespace Raycynix.Extensions.Serilog.Elastic.Configurations;

/// <summary>
/// Contains optional settings for the Elastic sink channel buffers.
/// Null values preserve the official sink defaults.
/// </summary>
public sealed class ElasticBufferOptions
{
    /// <summary>
    /// Gets or sets the maximum number of export retries.
    /// </summary>
    public int? ExportMaxRetries { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of concurrent export operations.
    /// </summary>
    public int? ExportMaxConcurrency { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of events accepted by the inbound buffer.
    /// </summary>
    public int? InboundBufferMaxSize { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of events contained in one export batch.
    /// </summary>
    public int? OutboundBufferMaxSize { get; set; }

    /// <summary>
    /// Gets or sets the maximum time events can remain in the outbound buffer
    /// before an export is attempted.
    /// </summary>
    public TimeSpan? OutboundBufferMaxLifetime { get; set; }

    /// <summary>
    /// Gets or sets the behavior used when the bounded channel is full.
    /// </summary>
    public BoundedChannelFullMode? FullMode { get; set; }
}
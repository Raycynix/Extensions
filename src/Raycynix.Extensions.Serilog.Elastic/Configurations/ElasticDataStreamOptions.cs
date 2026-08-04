namespace Raycynix.Extensions.Serilog.Elastic.Configurations;

/// <summary>
/// Contains Elastic data stream naming settings.
/// </summary>
public sealed class ElasticDataStreamOptions
{
    /// <summary>
    /// Gets or sets the data stream type.
    /// </summary>
    /// <remarks>
    /// The conventional value for application logs is <c>logs</c>.
    /// </remarks>
    public string Type { get; set; } = "logs";

    /// <summary>
    /// Gets or sets the data stream dataset.
    /// When empty, the Raycynix service name is used.
    /// </summary>
    public string? Dataset { get; set; }

    /// <summary>
    /// Gets or sets the data stream namespace.
    /// When empty, the Raycynix environment is used.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Gets or sets the optional index lifecycle management policy.
    /// </summary>
    public string? IlmPolicy { get; set; }
}
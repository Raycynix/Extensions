namespace Raycynix.Extensions.Serilog.Elastic.Enums;

/// <summary>
/// Defines how the Elastic sink connects to the Elastic deployment.
/// </summary>
public enum ElasticConnectionMode
{
    /// <summary>
    /// Connects directly to one or more Elasticsearch nodes.
    /// </summary>
    Elasticsearch = 0,

    /// <summary>
    /// Connects to an Elastic Cloud deployment using its Cloud ID.
    /// </summary>
    ElasticCloud = 1
}
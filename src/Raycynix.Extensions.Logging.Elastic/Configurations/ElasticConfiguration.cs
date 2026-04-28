namespace Raycynix.Extensions.Logging.Elastic.Configurations;

/// <summary>
/// Represents Elasticsearch sink settings for Raycynix logging.
/// </summary>
public class ElasticConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether Elasticsearch logging is enabled.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the Elasticsearch endpoint.
    /// </summary>
    public string Url { get; set; } = "http://localhost:9200";

    /// <summary>
    /// Validates the Elasticsearch logging configuration.
    /// </summary>
    public void Validate()
    {
        if (Enabled && !Uri.TryCreate(Url, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("Logging Elasticsearch URL must be a valid absolute URI when Elastic logging is enabled.");
        }
    }
}

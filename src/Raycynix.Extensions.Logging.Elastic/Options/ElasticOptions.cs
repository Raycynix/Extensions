namespace Raycynix.Extensions.Logging.Elastic.Options;

/// <summary>
/// Represents Elasticsearch sink options for Raycynix logging.
/// </summary>
public sealed class ElasticOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Elasticsearch logging is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the Elasticsearch endpoint.
    /// </summary>
    public string Url { get; set; } = "http://localhost:9200";

    /// <summary>
    /// Validates the Elasticsearch logging options.
    /// </summary>
    public void Validate()
    {
        if (Enabled &&
            (!Uri.TryCreate(Url, UriKind.Absolute, out var uri) ||
             (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)))
        {
            throw new InvalidOperationException(
                "Logging Elasticsearch URL must be a valid HTTP or HTTPS absolute URI when Elastic logging is enabled.");
        }
    }
}

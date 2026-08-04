namespace Raycynix.Extensions.Serilog.Elastic.Configurations;

/// <summary>
/// Contains optional proxy settings for the Elastic transport.
/// </summary>
public sealed class ElasticProxyOptions
{
    /// <summary>
    /// Gets or sets the proxy endpoint.
    /// </summary>
    public Uri? Url { get; set; }

    /// <summary>
    /// Gets or sets the optional proxy username.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the optional proxy password.
    /// </summary>
    public string? Password { get; set; }
}
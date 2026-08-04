using Raycynix.Extensions.Serilog.Elastic.Enums;

namespace Raycynix.Extensions.Serilog.Elastic.Configurations;

/// <summary>
/// Contains Elastic transport authentication settings.
/// </summary>
public sealed class ElasticAuthenticationOptions
{
    /// <summary>
    /// Gets or sets the authentication mechanism.
    /// </summary>
    public ElasticAuthenticationMode Mode { get; set; }

    /// <summary>
    /// Gets or sets the encoded Elastic API key.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the username used for basic authentication.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the password used for basic authentication.
    /// </summary>
    public string? Password { get; set; }
}
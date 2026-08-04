namespace Raycynix.Extensions.Serilog.Elastic.Enums;

/// <summary>
/// Defines the authentication mechanism used by the Elastic transport.
/// </summary>
public enum ElasticAuthenticationMode
{
    /// <summary>
    /// Does not configure transport authentication.
    /// </summary>
    None = 0,

    /// <summary>
    /// Authenticates using an Elastic API key.
    /// </summary>
    ApiKey = 1,

    /// <summary>
    /// Authenticates using a username and password.
    /// </summary>
    Basic = 2
}
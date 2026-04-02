namespace Raycynix.Extensions.Messaging.Configurations;

/// <summary>
/// Configures JSON message serialization behavior.
/// </summary>
public sealed class JsonMessagingConfiguration
{
    /// <summary>
    /// Gets a value indicating whether property names use camelCase.
    /// </summary>
    public bool UseCamelCase { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether JSON is pretty-printed.
    /// </summary>
    public bool WriteIndented { get; set; }
}

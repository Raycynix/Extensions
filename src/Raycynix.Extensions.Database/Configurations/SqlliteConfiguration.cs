namespace Raycynix.Extensions.Database.Configurations;

/// <summary>
/// Represents SQLite-specific connection settings.
/// </summary>
public class SqlliteConfiguration
{
    /// <summary>
    /// Gets the SQLite open mode.
    /// </summary>
    public string? Mode { get; init; }

    /// <summary>
    /// Gets the SQLite cache mode.
    /// </summary>
    public string? Cache { get; init; }

    /// <summary>
    /// Gets the command timeout in seconds.
    /// </summary>
    public int? CommandTimeoutSeconds { get; init; }
}

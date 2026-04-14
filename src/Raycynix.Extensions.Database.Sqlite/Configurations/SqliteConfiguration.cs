namespace Raycynix.Extensions.Database.Sqlite.Configurations;

/// <summary>
/// Represents SQLite-specific connection settings.
/// </summary>
public class SqliteConfiguration
{
    /// <summary>
    /// Gets the SQLite open mode.
    /// </summary>
    public string? Mode { get; set; }

    /// <summary>
    /// Gets the SQLite cache mode.
    /// </summary>
    public string? Cache { get; set; }

    /// <summary>
    /// Gets the command timeout in seconds.
    /// </summary>
    public int? CommandTimeoutSeconds { get; set; }
}

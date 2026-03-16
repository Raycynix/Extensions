namespace Raycynix.Extensions.Database.Configurations;

/// <summary>
/// Represents the configuration settings specific to SQLite databases.
/// This class is used to provide SQLite-related configuration details when setting up a database connection.
/// </summary>
public class SqlliteConfiguration
{
    /// <summary>
    /// Gets or sets the file mode used by SQLite.
    /// </summary>
    public string? Mode { get; init; }

    /// <summary>
    /// Gets or sets the cache mode used by SQLite.
    /// </summary>
    public string? Cache { get; init; }

    /// <summary>
    /// Gets or sets the command timeout in seconds.
    /// </summary>
    public int? CommandTimeoutSeconds { get; init; }
}
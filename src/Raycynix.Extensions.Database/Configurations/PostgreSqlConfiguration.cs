namespace Raycynix.Extensions.Database.Configurations;

/// <summary>
/// Represents the configuration settings specific to PostgreSQL databases.
/// This class is used to provide PostgreSQL-specific configuration details
/// when setting up a database connection.
/// </summary>
public class PostgreSqlConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether connection pooling is enabled.
    /// </summary>
    public bool Pooling { get; init; } = true;

    /// <summary>
    /// Gets or sets the minimum pool size.
    /// </summary>
    public int? MinimumPoolSize { get; init; }

    /// <summary>
    /// Gets or sets the maximum pool size.
    /// </summary>
    public int? MaximumPoolSize { get; init; }

    /// <summary>
    /// Gets or sets the command timeout in seconds.
    /// </summary>
    public int? CommandTimeoutSeconds { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether detailed database errors are enabled.
    /// </summary>
    public bool IncludeErrorDetail { get; init; } = false;
}
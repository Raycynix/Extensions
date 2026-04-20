namespace Raycynix.Extensions.Database.PostgreSql.Configurations;

/// <summary>
/// Represents PostgreSQL-specific connection settings.
/// </summary>
public class PostgreSqlConfiguration
{
    /// <summary>
    /// Gets a value indicating whether connection pooling is enabled.
    /// </summary>
    public bool Pooling { get; set; } = true;

    /// <summary>
    /// Gets the minimum pool size.
    /// </summary>
    public int? MinimumPoolSize { get; set; }

    /// <summary>
    /// Gets the maximum pool size.
    /// </summary>
    public int? MaximumPoolSize { get; set; }

    /// <summary>
    /// Gets the command timeout in seconds.
    /// </summary>
    public int? CommandTimeoutSeconds { get; set; }

    /// <summary>
    /// Gets a value indicating whether detailed provider errors are included.
    /// </summary>
    public bool IncludeErrorDetail { get; set; } = false;
}

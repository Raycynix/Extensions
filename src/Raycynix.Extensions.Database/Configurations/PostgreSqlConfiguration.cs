namespace Raycynix.Extensions.Database.Configurations;

/// <summary>
/// Represents PostgreSQL-specific connection settings.
/// </summary>
public class PostgreSqlConfiguration
{
    /// <summary>
    /// Gets a value indicating whether connection pooling is enabled.
    /// </summary>
    public bool Pooling { get; init; } = true;

    /// <summary>
    /// Gets the minimum pool size.
    /// </summary>
    public int? MinimumPoolSize { get; init; }

    /// <summary>
    /// Gets the maximum pool size.
    /// </summary>
    public int? MaximumPoolSize { get; init; }

    /// <summary>
    /// Gets the command timeout in seconds.
    /// </summary>
    public int? CommandTimeoutSeconds { get; init; }

    /// <summary>
    /// Gets a value indicating whether detailed provider errors are included.
    /// </summary>
    public bool IncludeErrorDetail { get; init; } = false;
}

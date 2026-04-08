namespace Raycynix.Extensions.Database.MySql.Configurations;

/// <summary>
/// Represents MySQL-specific connection settings.
/// </summary>
public class MySqlConfiguration
{
    /// <summary>
    /// Gets the command timeout in seconds.
    /// </summary>
    public int? CommandTimeoutSeconds { get; init; }

    /// <summary>
    /// Gets a value indicating whether user variables are allowed.
    /// </summary>
    public bool AllowUserVariables { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether connection pooling is enabled.
    /// </summary>
    public bool Pooling { get; init; } = true;
}

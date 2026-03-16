namespace Raycynix.Extensions.Database.Configurations;

/// <summary>
/// Represents the configuration settings specific to a MySQL database.
/// This class may optionally contain properties to define various settings
/// required to configure and manage connections to a MySQL database instance.
/// </summary>
public class MySqlConfiguration
{
    /// <summary>
    /// Gets or sets the command timeout in seconds.
    /// </summary>
    public int? CommandTimeoutSeconds { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether user variables are allowed.
    /// </summary>
    public bool AllowUserVariables { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether connection pooling is enabled.
    /// </summary>
    public bool Pooling { get; init; } = true;
}
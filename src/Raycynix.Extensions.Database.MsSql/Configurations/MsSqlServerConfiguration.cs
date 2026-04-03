namespace Raycynix.Extensions.Database.MsSql.Configurations;

/// <summary>
/// Represents SQL Server-specific connection settings.
/// </summary>
public class MsSqlServerConfiguration
{
    /// <summary>
    /// Gets a value indicating whether the server certificate should be trusted.
    /// </summary>
    public bool TrustServerCertificate { get; init; } = true;

    /// <summary>
    /// Gets the command timeout in seconds.
    /// </summary>
    public int? CommandTimeoutSeconds { get; init; }

    /// <summary>
    /// Gets a value indicating whether multiple active result sets are enabled.
    /// </summary>
    public bool MultipleActiveResultSets { get; init; } = false;
}

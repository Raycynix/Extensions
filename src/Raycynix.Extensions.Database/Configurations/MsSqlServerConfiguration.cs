namespace Raycynix.Extensions.Database.Configurations;

/// <summary>
/// Represents the specific configuration settings for connecting to a Microsoft SQL Server database.
/// This class is used to define properties and options unique to MS SQL Server,
/// allowing customization and fine-tuning of the connection and behavior related to the database.
/// </summary>
public class MsSqlServerConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether the server certificate should be trusted.
    /// </summary>
    public bool TrustServerCertificate { get; init; } = true;

    /// <summary>
    /// Gets or sets the command timeout in seconds.
    /// </summary>
    public int? CommandTimeoutSeconds { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether multiple active result sets are enabled.
    /// </summary>
    public bool MultipleActiveResultSets { get; init; } = false;
}
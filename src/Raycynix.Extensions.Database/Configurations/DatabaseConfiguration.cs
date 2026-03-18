using Raycynix.Extensions.Database.Models;

namespace Raycynix.Extensions.Database.Configurations;

/// <summary>
/// Represents the database settings used by the Raycynix database extensions.
/// </summary>
public class DatabaseConfiguration
{
    /// <summary>
    /// Gets the raw connection string.
    /// </summary>
    public string? ConnectionString { get; init; }

    /// <summary>
    /// Gets the structured connection settings used when a raw connection string is not supplied.
    /// </summary>
    public ConnectionConfiguration? ConnectionConfiguration { get; init; }

    /// <summary>
    /// Gets the selected database provider.
    /// </summary>
    public DatabaseProvider Provider { get; init; } = DatabaseProvider.PostgreSql;

    /// <summary>
    /// Gets a value indicating whether EF Core migrations should be applied during initialization.
    /// </summary>
    public bool UseMigrations { get; init; }

    /// <summary>
    /// Gets a value indicating whether the database should be created when it does not exist.
    /// </summary>
    public bool EnsureCreated { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether entity seed logic should run during model creation.
    /// </summary>
    public bool EnableSeed { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether EF Core lazy loading is enabled for the shared context.
    /// </summary>
    public bool EnableLazyLoading { get; init; }

    /// <summary>
    /// Gets a value indicating whether EF Core automatic change detection is enabled.
    /// </summary>
    public bool EnableAutoDetectChanges { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether queries are tracked by default.
    /// </summary>
    public bool UseQueryTrackingByDefault { get; init; } = true;

    /// <summary>
    /// Gets the maximum number of retry attempts for transient database failures.
    /// </summary>
    public int RetryCount { get; init; } = 5;

    /// <summary>
    /// Gets the delay, in seconds, between retry attempts.
    /// </summary>
    public int RetryDelaySeconds { get; init; } = 10;

    /// <summary>
    /// Gets the PostgreSQL-specific settings.
    /// </summary>
    public PostgreSqlConfiguration? PostgreSqlConfiguration { get; init; }

    /// <summary>
    /// Gets the SQL Server-specific settings.
    /// </summary>
    public MsSqlServerConfiguration? MsSqlServerConfiguration { get; init; }

    /// <summary>
    /// Gets the MySQL-specific settings.
    /// </summary>
    public MySqlConfiguration? MySqlConfiguration { get; init; }

    /// <summary>
    /// Gets the SQLite-specific settings.
    /// </summary>
    public SqlliteConfiguration? SqlliteConfiguration { get; init; }

    /// <summary>
    /// Validates the configuration and throws when incompatible or incomplete values are provided.
    /// </summary>
    public void Validate()
    {
        if (RetryCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(RetryCount), "Retry count cannot be negative.");
        }

        if (RetryDelaySeconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(RetryDelaySeconds), "Retry delay cannot be negative.");
        }

        if (EnsureCreated && UseMigrations)
        {
            throw new InvalidOperationException(
                "EnsureCreated and UseMigrations cannot both be enabled at the same time.");
        }

        var hasConnectionString = !string.IsNullOrWhiteSpace(ConnectionString);
        var hasConnectionConfig = ConnectionConfiguration is not null;

        if (!hasConnectionString && !hasConnectionConfig)
        {
            throw new InvalidOperationException(
                "Either ConnectionString or ConnectionConfiguration must be provided.");
        }

        if (hasConnectionConfig)
        {
            ConnectionConfiguration!.Validate(Provider.ToString());
        }
    }
}

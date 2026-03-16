using Raycynix.Extensions.Database.Models;

namespace Raycynix.Extensions.Database.Configurations;

/// <summary>
/// Represents the configuration settings required for database operations within an application.
/// </summary>
/// <remarks>
/// This class encapsulates various properties that can be used to configure database connections,
/// providers, retry mechanisms, as well as advanced settings specific to certain database platforms.
/// </remarks>
public class DatabaseConfiguration
{
    /// <summary>
    /// Defines the database connection string used to establish a connection to a data source.
    /// This property contains the necessary information, such as the data source location,
    /// credentials, and any additional parameters required for connecting to the target database system.
    /// </summary>
    public string? ConnectionString { get; init; }

    /// <summary>
    /// Represents the configuration settings required for establishing a database connection.
    /// This property encapsulates details such as host, port, database name, username, and password,
    /// which are necessary for constructing a valid connection string specific to the database provider.
    /// </summary>
    public ConnectionConfiguration? ConnectionConfiguration { get; init; }

    /// <summary>
    /// Determines the type of database provider to be used for configuring the application's database connection.
    /// This property allows selecting from supported providers like PostgreSQL, MySQL, SQLite, or Microsoft SQL Server,
    /// enabling tailored configurations for the underlying database engine.
    /// </summary>
    public DatabaseProvider Provider { get; init; } = DatabaseProvider.PostgreSql;

    /// <summary>
    /// Specifies whether the application's database schema should be managed using migrations.
    /// This property enables the automated application of incremental changes to the database
    /// schema, ensuring it remains synchronized with the application's data model.
    /// </summary>
    public bool UseMigrations { get; init; }

    /// <summary>
    /// Indicates whether the database should be created automatically if it does not already exist.
    /// This property is checked during the initialization process to determine if the database
    /// creation logic should be executed. It simplifies setup in scenarios where a database needs
    /// to be ensured without applying migrations.
    /// </summary>
    public bool EnsureCreated { get; init; } = true;

    /// <summary>
    /// Determines whether database seeding operations should be executed after the model configuration is applied.
    /// This property allows the injection of initial data into the database, useful for testing, prototyping,
    /// or establishing the default application state during setup.
    /// </summary>
    public bool EnableSeed { get; init; } = true;

    /// <summary>
    /// Specifies the maximum number of retry attempts to perform when a transient
    /// fault occurs during database operations. This property is used to configure
    /// resiliency by allowing the system to retry failed operations up to the defined
    /// number of times before giving up.
    /// </summary>
    public int RetryCount { get; init; } = 5;

    /// <summary>
    /// Defines the delay, in seconds, between retry attempts when a database operation fails.
    /// This property is used to specify the interval before the next retry in scenarios
    /// where transient faults occur, enabling better resiliency in database interactions.
    /// </summary>
    public int RetryDelaySeconds { get; init; } = 10;

    /// <summary>
    /// Represents the configuration settings specific to PostgreSQL databases.
    /// This configuration class is designed to handle PostgreSQL-specific options
    /// and parameters, allowing for fine-tuned control over connection and operational
    /// aspects related to PostgreSQL database interactions.
    /// </summary>
    public PostgreSqlConfiguration? PostgreSqlConfiguration { get; init; }

    /// <summary>
    /// Defines the configuration settings specific to Microsoft SQL Server databases.
    /// This configuration class is intended to encapsulate properties and options
    /// that tailor the behavior of connections and operations specifically for
    /// Microsoft SQL Server, providing flexibility for managing database interactions.
    /// </summary>
    public MsSqlServerConfiguration? MsSqlServerConfiguration { get; init; }

    /// <summary>
    /// Represents the configuration settings specific to a MySQL database.
    /// This configuration class is designed to contain properties necessary
    /// for establishing and managing connections to a MySQL database instance,
    /// such as connection parameters, authentication details, and other
    /// database-specific settings.
    /// </summary>
    public MySqlConfiguration? MySqlConfiguration { get; init; }

    /// <summary>
    /// Represents the configuration settings specific to SQLite databases.
    /// This class is used to configure and manage SQLite-related database connection settings
    /// in the context of the database configuration workflow.
    /// </summary>
    public SqlliteConfiguration? SqlliteConfiguration { get; init; }

    /// <summary>
    /// Validates the current configuration and throws an exception if it is invalid.
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
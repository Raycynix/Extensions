using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Npgsql;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Configurations;
using Raycynix.Extensions.Database.Implementations;
using Raycynix.Extensions.Database.Models;
using MySqlConfiguration = Raycynix.Extensions.Database.Configurations.MySqlConfiguration;

namespace Raycynix.Extensions.Database;

/// <summary>
/// Provides extension methods for configuring and adding database-related services to the dependency injection container.
/// </summary>
/// <remarks>
/// This class is designed to abstract the process of configuring and registering database dependencies,
/// including database contexts, initializers, and configuration settings. It supports multiple database providers
/// such as PostgreSQL, SQL Server, MySQL, and SQLite. The connection string and other settings can be specified
/// via the provided configuration.
/// </remarks>
/// <example>
/// To use this class, call `AddRaycynixDatabase` during application startup and provide the necessary configuration settings.
/// </example>
/// <threadsafety>
/// This class is thread-safe as it only provides static extension methods for registration purposes.
/// </threadsafety>
public static class Database
{
    /// <summary>
    /// Adds the Raycynix database and its related services to the dependency injection container.
    /// </summary>
    /// <param name="services">
    /// The <see cref="IServiceCollection"/> to which the database services will be added.
    /// </param>
    /// <param name="configuration">
    /// The <see cref="IConfiguration"/> instance containing application configuration settings.
    /// </param>
    /// <param name="setup">
    /// An optional action to configure additional <see cref="DatabaseConfiguration"/> settings.
    /// </param>
    /// <returns>
    /// The updated <see cref="IServiceCollection"/> including the configured database services.
    /// </returns>
    public static IServiceCollection AddRaycynixDatabase(this IServiceCollection services, IConfiguration configuration,
        Action<DatabaseConfiguration>? setup = null)
    {
        var config = new DatabaseConfiguration();
        configuration.GetSection(nameof(DatabaseConfiguration)).Bind(config);

        setup?.Invoke(config);
        config.Validate();

        var finalString = ResolveConnection(config);
        var callerAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();

        services.AddSingleton(config);
        services.AddSingleton(callerAssembly);
        services.AddSingleton<IDatabaseInitializer, DatabaseInitializer>();

        services.AddDbContextPool<DatabaseContext>(options =>
        {
            switch (config.Provider)
            {
                case DatabaseProvider.PostgreSql:
                    options.UseNpgsql(finalString, npgsqlOptions =>
                    {
                        npgsqlOptions.EnableRetryOnFailure(
                            config.RetryCount,
                            TimeSpan.FromSeconds(config.RetryDelaySeconds),
                            null);

                        npgsqlOptions.MigrationsAssembly(callerAssembly.GetName().Name);

                        var providerConfig = config.PostgreSqlConfiguration;
                        if (providerConfig?.CommandTimeoutSeconds is not null)
                        {
                            npgsqlOptions.CommandTimeout(providerConfig.CommandTimeoutSeconds.Value);
                        }
                    });
                    break;

                case DatabaseProvider.MsSqlServer:
                    options.UseSqlServer(finalString, sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            config.RetryCount,
                            TimeSpan.FromSeconds(config.RetryDelaySeconds),
                            null);

                        sqlOptions.MigrationsAssembly(callerAssembly.GetName().Name);

                        var providerConfig = config.MsSqlServerConfiguration;
                        if (providerConfig?.CommandTimeoutSeconds is not null)
                        {
                            sqlOptions.CommandTimeout(providerConfig.CommandTimeoutSeconds.Value);
                        }
                    });
                    break;

                case DatabaseProvider.MySql:
                    options.UseMySQL(finalString, mySqlOptions =>
                    {
                        mySqlOptions.EnableRetryOnFailure(
                            config.RetryCount,
                            TimeSpan.FromSeconds(config.RetryDelaySeconds),
                            null);

                        mySqlOptions.MigrationsAssembly(callerAssembly.GetName().Name);

                        var providerConfig = config.MySqlConfiguration;
                        if (providerConfig?.CommandTimeoutSeconds is not null)
                        {
                            mySqlOptions.CommandTimeout(providerConfig.CommandTimeoutSeconds.Value);
                        }
                    });
                    break;

                case DatabaseProvider.Sqlite:
                default:
                    options.UseSqlite(finalString, sqliteOptions =>
                    {
                        sqliteOptions.MigrationsAssembly(callerAssembly.GetName().Name);

                        var providerConfig = config.SqlliteConfiguration;
                        if (providerConfig?.CommandTimeoutSeconds is not null)
                        {
                            sqliteOptions.CommandTimeout(providerConfig.CommandTimeoutSeconds.Value);
                        }
                    });
                    break;
            }
        });

        return services;
    }

    private static string ResolveConnection(DatabaseConfiguration config)
    {
        if (!string.IsNullOrWhiteSpace(config.ConnectionString))
        {
            return config.ConnectionString;
        }

        var connection = config.ConnectionConfiguration
                         ?? throw new ArgumentException("Connection configuration is missing.");

        return config.Provider switch
        {
            DatabaseProvider.PostgreSql => BuildPostgreSqlConnectionString(connection, config.PostgreSqlConfiguration),
            DatabaseProvider.MsSqlServer => BuildSqlServerConnectionString(connection, config.MsSqlServerConfiguration),
            DatabaseProvider.MySql => BuildMySqlConnectionString(connection, config.MySqlConfiguration),
            DatabaseProvider.Sqlite => BuildSqliteConnectionString(connection, config.SqlliteConfiguration),
            _ => throw new NotSupportedException($"Resolving for {config.Provider} not realised.")
        };
    }

    private static string BuildPostgreSqlConnectionString(
        ConnectionConfiguration connection,
        PostgreSqlConfiguration? providerConfig)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = connection.Host,
            Port = connection.Port ?? 5432,
            Database = connection.Name,
            Username = connection.Username,
            Password = connection.Password,
            Pooling = providerConfig?.Pooling ?? true,
            IncludeErrorDetail = providerConfig?.IncludeErrorDetail ?? false
        };

        if (providerConfig?.MinimumPoolSize is not null)
        {
            builder.MinPoolSize = providerConfig.MinimumPoolSize.Value;
        }

        if (providerConfig?.MaximumPoolSize is not null)
        {
            builder.MaxPoolSize = providerConfig.MaximumPoolSize.Value;
        }

        if (providerConfig?.CommandTimeoutSeconds is not null)
        {
            builder.CommandTimeout = providerConfig.CommandTimeoutSeconds.Value;
        }

        return builder.ConnectionString;
    }

    private static string BuildSqlServerConnectionString(
        ConnectionConfiguration connection,
        MsSqlServerConfiguration? providerConfig)
    {
        var builder = new SqlConnectionStringBuilder()
        {
            DataSource = connection.Host,
            InitialCatalog = connection.Name,
            UserID = connection.Username,
            Password = connection.Password,
            TrustServerCertificate = providerConfig?.TrustServerCertificate ?? true,
            MultipleActiveResultSets = providerConfig?.MultipleActiveResultSets ?? false
        };

        return builder.ConnectionString;
    }

    private static string BuildMySqlConnectionString(
        ConnectionConfiguration connection,
        MySqlConfiguration? providerConfig)
    {
        var builder = new MySqlConnectionStringBuilder
        {
            Server = connection.Host,
            Port = (uint)(connection.Port ?? 3306),
            Database = connection.Name,
            UserID = connection.Username,
            Password = connection.Password,
            AllowUserVariables = providerConfig?.AllowUserVariables ?? true,
            Pooling = providerConfig?.Pooling ?? true
        };

        return builder.ConnectionString;
    }

    private static string BuildSqliteConnectionString(
        ConnectionConfiguration connection,
        SqlliteConfiguration? providerConfig)
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = connection.Name
        };

        if (!string.IsNullOrWhiteSpace(providerConfig?.Mode))
        {
            builder.Mode = Enum.Parse<SqliteOpenMode>(providerConfig.Mode, ignoreCase: true);
        }

        if (!string.IsNullOrWhiteSpace(providerConfig?.Cache))
        {
            builder.Cache = Enum.Parse<SqliteCacheMode>(providerConfig.Cache, ignoreCase: true);
        }

        return builder.ToString();
    }
}
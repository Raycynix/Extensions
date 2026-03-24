using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using MySql.Data.MySqlClient;
using Npgsql;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Configurations;
using Raycynix.Extensions.Database.Implementations;
using Raycynix.Extensions.Database.Internal;
using Raycynix.Extensions.Database.Models;
using MySqlConfiguration = Raycynix.Extensions.Database.Configurations.MySqlConfiguration;

namespace Raycynix.Extensions.Database;

/// <summary>
/// Provides service registration extensions for the Raycynix database package.
/// </summary>
public static class Database
{
    /// <summary>
    /// Registers the database context, initializer, and provider-specific EF Core configuration.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="configuration">The application configuration used to bind <see cref="DatabaseConfiguration"/>.</param>
    /// <param name="setup">An optional callback for adjusting the bound database configuration.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixDatabase(this IServiceCollection services, IConfiguration configuration,
        Action<DatabaseConfiguration>? setup = null)
    {
        var callerAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();

        services.AddRaycynixConfiguration<DatabaseConfiguration>(
            configuration,
            configurePostBind: setup);
        services.AddRaycynixConfigurationValidator<DatabaseConfiguration, DatabaseConfigurationValidator>();
        services.AddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IConfigurationAccessor<DatabaseConfiguration>>().Current);
        services.AddSingleton(callerAssembly);
        services.AddSingleton<DatabaseObservability>();
        services.AddSingleton<IDatabaseInitializer, DatabaseInitializer>();

        services.AddDbContextPool<DatabaseContext>((serviceProvider, options) =>
        {
            var config = serviceProvider.GetRequiredService<DatabaseConfiguration>();
            var finalString = ResolveConnection(config);

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

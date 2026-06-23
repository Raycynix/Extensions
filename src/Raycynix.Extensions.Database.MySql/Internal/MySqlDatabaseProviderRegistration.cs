using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Configurations;
using MySqlConfiguration = Raycynix.Extensions.Database.MySql.Configurations.MySqlConfiguration;

namespace Raycynix.Extensions.Database.MySql.Internal;

/// <summary>
/// Implements MySQL-specific connection and EF Core configuration for the shared database context.
/// </summary>
internal sealed class MySqlDatabaseProviderRegistration(
    ILogger<MySqlDatabaseProviderRegistration>? logger = null) : IDatabaseProviderRegistration
{
    /// <inheritdoc />
    public string ProviderName => "mysql";

    /// <inheritdoc />
    public string ResolveConnectionString(DatabaseConfiguration configuration, IServiceProvider serviceProvider)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            logger?.LogDebug("Using configured raw MySQL connection string.");
            return configuration.ConnectionString;
        }

        var connection = configuration.ConnectionConfiguration
                         ?? throw new ArgumentException("Connection configuration is missing.");
        
        var providerConfig = serviceProvider
            .GetService(typeof(IConfigurationAccessor<MySqlConfiguration>)) as IConfigurationAccessor<MySqlConfiguration>;

        var settings = providerConfig?.Current;
        var builder = new MySqlConnectionStringBuilder
        {
            Server = connection.Host,
            Port = (uint)(connection.Port ?? 3306),
            Database = connection.Name,
            UserID = connection.Username,
            Password = connection.Password,
            AllowUserVariables = settings?.AllowUserVariables ?? true,
            Pooling = settings?.Pooling ?? true
        };

        logger?.LogDebug(
            "Resolved MySQL connection string from structured configuration. Pooling: {Pooling}, AllowUserVariables: {AllowUserVariables}, CommandTimeoutConfigured: {CommandTimeoutConfigured}.",
            builder.Pooling,
            builder.AllowUserVariables,
            settings?.CommandTimeoutSeconds is not null);

        return builder.ConnectionString;
    }

    /// <inheritdoc />
    public void Configure(
        DbContextOptionsBuilder options,
        string connectionString,
        DatabaseConfiguration configuration,
        Assembly migrationsAssembly,
        IServiceProvider serviceProvider)
    {
        var providerConfig = serviceProvider
            .GetService(typeof(IConfigurationAccessor<MySqlConfiguration>)) as IConfigurationAccessor<MySqlConfiguration>;
        
        options.UseMySQL(connectionString, mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(
                configuration.RetryCount,
                TimeSpan.FromSeconds(configuration.RetryDelaySeconds),
                null);

            mySqlOptions.MigrationsAssembly(migrationsAssembly.GetName().Name);

            var settings = providerConfig?.Current;
            if (settings?.CommandTimeoutSeconds is not null)
            {
                mySqlOptions.CommandTimeout(settings.CommandTimeoutSeconds.Value);
            }
        });

        logger?.LogDebug(
            "Configured EF Core MySQL provider. Migrations assembly: {MigrationsAssembly}, RetryCount: {RetryCount}, RetryDelaySeconds: {RetryDelaySeconds}.",
            migrationsAssembly.GetName().Name,
            configuration.RetryCount,
            configuration.RetryDelaySeconds);
    }

    /// <inheritdoc />
    public void Validate(DatabaseConfiguration configuration)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            logger?.LogDebug("Skipping structured MySQL validation because a raw connection string is configured.");
            return;
        }

        var connection = configuration.ConnectionConfiguration
                         ?? throw new InvalidOperationException("MySQL connection configuration is missing.");

        if (string.IsNullOrWhiteSpace(connection.Host))
        {
            throw new InvalidOperationException("MySQL connection requires a host.");
        }

        if (string.IsNullOrWhiteSpace(connection.Name))
        {
            throw new InvalidOperationException("MySQL connection requires a database name.");
        }

        if (string.IsNullOrWhiteSpace(connection.Username))
        {
            throw new InvalidOperationException("MySQL connection requires a username.");
        }

        logger?.LogDebug("MySQL structured connection configuration validated.");
    }
}

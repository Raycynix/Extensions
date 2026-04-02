using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Configurations;
using Raycynix.Extensions.Database.Enums;

namespace Raycynix.Extensions.Database.Internal;

/// <summary>
/// Implements MySQL-specific connection and EF Core configuration for the shared database context.
/// </summary>
internal sealed class MySqlDatabaseProviderRegistration : IDatabaseProviderRegistration
{
    /// <inheritdoc />
    public DatabaseProvider Provider => DatabaseProvider.MySql;

    /// <inheritdoc />
    public string ResolveConnectionString(DatabaseConfiguration configuration, IServiceProvider serviceProvider)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            return configuration.ConnectionString;
        }

        var connection = configuration.ConnectionConfiguration
                         ?? throw new ArgumentException("Connection configuration is missing.");

        var providerConfig = configuration.MySqlConfiguration;
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

    /// <inheritdoc />
    public void Configure(
        DbContextOptionsBuilder options,
        string connectionString,
        DatabaseConfiguration configuration,
        Assembly migrationsAssembly,
        IServiceProvider serviceProvider)
    {
        options.UseMySQL(connectionString, mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(
                configuration.RetryCount,
                TimeSpan.FromSeconds(configuration.RetryDelaySeconds),
                null);

            mySqlOptions.MigrationsAssembly(migrationsAssembly.GetName().Name);

            var providerConfig = configuration.MySqlConfiguration;
            if (providerConfig?.CommandTimeoutSeconds is not null)
            {
                mySqlOptions.CommandTimeout(providerConfig.CommandTimeoutSeconds.Value);
            }
        });
    }
}

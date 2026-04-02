using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Configurations;
using Raycynix.Extensions.Database.Enums;

namespace Raycynix.Extensions.Database.Internal;

/// <summary>
/// Implements SQLite-specific connection and EF Core configuration for the shared database context.
/// </summary>
internal sealed class SqliteDatabaseProviderRegistration : IDatabaseProviderRegistration
{
    /// <inheritdoc />
    public DatabaseProvider Provider => DatabaseProvider.Sqlite;

    /// <inheritdoc />
    public string ResolveConnectionString(DatabaseConfiguration configuration, IServiceProvider serviceProvider)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            return configuration.ConnectionString;
        }

        var connection = configuration.ConnectionConfiguration
                         ?? throw new ArgumentException("Connection configuration is missing.");

        var providerConfig = configuration.SqlliteConfiguration;
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

    /// <inheritdoc />
    public void Configure(
        DbContextOptionsBuilder options,
        string connectionString,
        DatabaseConfiguration configuration,
        Assembly migrationsAssembly,
        IServiceProvider serviceProvider)
    {
        options.UseSqlite(connectionString, sqliteOptions =>
        {
            sqliteOptions.MigrationsAssembly(migrationsAssembly.GetName().Name);

            var providerConfig = configuration.SqlliteConfiguration;
            if (providerConfig?.CommandTimeoutSeconds is not null)
            {
                sqliteOptions.CommandTimeout(providerConfig.CommandTimeoutSeconds.Value);
            }
        });
    }
}

using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Configurations;
using Raycynix.Extensions.Database.Sqlite.Configurations;

namespace Raycynix.Extensions.Database.Sqlite.Internal;

/// <summary>
/// Implements SQLite-specific connection and EF Core configuration for the shared database context.
/// </summary>
internal sealed class SqliteDatabaseProviderRegistration : IDatabaseProviderRegistration
{
    /// <inheritdoc />
    public string ProviderName => "sqlite";

    /// <inheritdoc />
    public string ResolveConnectionString(DatabaseConfiguration configuration, IServiceProvider serviceProvider)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            return configuration.ConnectionString;
        }

        var connection = configuration.ConnectionConfiguration
                         ?? throw new ArgumentException("Connection configuration is missing.");

        var providerConfig = serviceProvider.GetService(typeof(IConfigurationAccessor<SqliteConfiguration>)) as IConfigurationAccessor<SqliteConfiguration>;
        
        var settings = providerConfig?.Current;
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = connection.Name
        };

        if (!string.IsNullOrWhiteSpace(settings?.Mode))
        {
            builder.Mode = Enum.Parse<SqliteOpenMode>(settings.Mode, ignoreCase: true);
        }

        if (!string.IsNullOrWhiteSpace(settings?.Cache))
        {
            builder.Cache = Enum.Parse<SqliteCacheMode>(settings.Cache, ignoreCase: true);
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
        var providerConfig = serviceProvider.GetService(typeof(IConfigurationAccessor<SqliteConfiguration>)) as IConfigurationAccessor<SqliteConfiguration>;
        
        options.UseSqlite(connectionString, sqliteOptions =>
        {
            sqliteOptions.MigrationsAssembly(migrationsAssembly.GetName().Name);

            var settings = providerConfig?.Current;
            if (settings?.CommandTimeoutSeconds is not null)
            {
                sqliteOptions.CommandTimeout(settings.CommandTimeoutSeconds.Value);
            }
        });
    }
}

using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Configurations;
using MySqlConfiguration = Raycynix.Extensions.Database.MySql.Configurations.MySqlConfiguration;

namespace Raycynix.Extensions.Database.MySql.Internal;

/// <summary>
/// Implements MySQL-specific connection and EF Core configuration for the shared database context.
/// </summary>
internal sealed class MySqlDatabaseProviderRegistration : IDatabaseProviderRegistration
{
    /// <inheritdoc />
    public string ProviderName => "mysql";

    /// <inheritdoc />
    public string ResolveConnectionString(DatabaseConfiguration configuration, IServiceProvider serviceProvider)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
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
    }
}

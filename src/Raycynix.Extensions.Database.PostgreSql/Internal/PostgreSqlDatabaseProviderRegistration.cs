using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Configurations;
using Raycynix.Extensions.Database.PostgreSql.Configurations;

namespace Raycynix.Extensions.Database.PostgreSql.Internal;

/// <summary>
/// Implements PostgreSQL-specific connection and EF Core configuration for the shared database context.
/// </summary>
internal sealed class PostgreSqlDatabaseProviderRegistration : IDatabaseProviderRegistration
{
    /// <inheritdoc />
    public string ProviderName => "postgresql";

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
            .GetService(typeof(IConfigurationAccessor<PostgreSqlConfiguration>)) as IConfigurationAccessor<PostgreSqlConfiguration>;

        var settings = providerConfig?.Current;
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = connection.Host,
            Port = connection.Port ?? 5432,
            Database = connection.Name,
            Username = connection.Username,
            Password = connection.Password,
            Pooling = settings?.Pooling ?? true,
            IncludeErrorDetail = settings?.IncludeErrorDetail ?? false
        };

        if (settings?.MinimumPoolSize is not null)
        {
            builder.MinPoolSize = settings.MinimumPoolSize.Value;
        }

        if (settings?.MaximumPoolSize is not null)
        {
            builder.MaxPoolSize = settings.MaximumPoolSize.Value;
        }

        if (settings?.CommandTimeoutSeconds is not null)
        {
            builder.CommandTimeout = settings.CommandTimeoutSeconds.Value;
        }

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
            .GetService(typeof(IConfigurationAccessor<PostgreSqlConfiguration>)) as IConfigurationAccessor<PostgreSqlConfiguration>;

        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                configuration.RetryCount,
                TimeSpan.FromSeconds(configuration.RetryDelaySeconds),
                null);

            npgsqlOptions.MigrationsAssembly(migrationsAssembly.GetName().Name);

            var settings = providerConfig?.Current;
            if (settings?.CommandTimeoutSeconds is not null)
            {
                npgsqlOptions.CommandTimeout(settings.CommandTimeoutSeconds.Value);
            }
        });
    }

    /// <inheritdoc />
    public void Validate(DatabaseConfiguration configuration)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            return;
        }

        var connection = configuration.ConnectionConfiguration
                         ?? throw new InvalidOperationException("PostgreSQL connection configuration is missing.");

        if (string.IsNullOrWhiteSpace(connection.Host))
        {
            throw new InvalidOperationException("PostgreSQL connection requires a host.");
        }

        if (string.IsNullOrWhiteSpace(connection.Name))
        {
            throw new InvalidOperationException("PostgreSQL connection requires a database name.");
        }

        if (string.IsNullOrWhiteSpace(connection.Username))
        {
            throw new InvalidOperationException("PostgreSQL connection requires a username.");
        }
    }
}

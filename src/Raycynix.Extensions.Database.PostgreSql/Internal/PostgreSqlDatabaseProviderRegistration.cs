using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Configurations;
using Raycynix.Extensions.Database.PostgreSql.Configurations;

namespace Raycynix.Extensions.Database.PostgreSql.Internal;

/// <summary>
/// Implements PostgreSQL-specific connection and EF Core configuration for the shared database context.
/// </summary>
internal sealed class PostgreSqlDatabaseProviderRegistration(
    ILogger<PostgreSqlDatabaseProviderRegistration>? logger = null) : IDatabaseProviderRegistration
{
    /// <inheritdoc />
    public string ProviderName => "postgresql";

    /// <inheritdoc />
    public string ResolveConnectionString(DatabaseConfiguration configuration, IServiceProvider serviceProvider)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            logger?.LogDebug("Using configured raw PostgreSQL connection string.");
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

        logger?.LogDebug(
            "Resolved PostgreSQL connection string from structured configuration. Pooling: {Pooling}, MinimumPoolSizeConfigured: {MinimumPoolSizeConfigured}, MaximumPoolSizeConfigured: {MaximumPoolSizeConfigured}, CommandTimeoutConfigured: {CommandTimeoutConfigured}.",
            builder.Pooling,
            settings?.MinimumPoolSize is not null,
            settings?.MaximumPoolSize is not null,
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

        logger?.LogDebug(
            "Configured EF Core PostgreSQL provider. Migrations assembly: {MigrationsAssembly}, RetryCount: {RetryCount}, RetryDelaySeconds: {RetryDelaySeconds}.",
            migrationsAssembly.GetName().Name,
            configuration.RetryCount,
            configuration.RetryDelaySeconds);
    }

    /// <inheritdoc />
    public void Validate(DatabaseConfiguration configuration)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            logger?.LogDebug("Skipping structured PostgreSQL validation because a raw connection string is configured.");
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

        logger?.LogDebug("PostgreSQL structured connection configuration validated.");
    }
}

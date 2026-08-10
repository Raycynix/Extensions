using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Options;
using Raycynix.Extensions.Database.MsSql.Options;

namespace Raycynix.Extensions.Database.MsSql.Internal;

/// <summary>
/// Implements SQL Server-specific connection and EF Core configuration for the shared database context.
/// </summary>
internal sealed class MsSqlServerDatabaseProviderRegistration(
    ILogger<MsSqlServerDatabaseProviderRegistration>? logger = null) : IDatabaseProviderRegistration
{
    /// <inheritdoc />
    public string ProviderName => "sqlserver";

    /// <inheritdoc />
    public string ResolveConnectionString(DatabaseOptions configuration, IServiceProvider serviceProvider)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            logger?.LogDebug("Using configured raw SQL Server connection string.");
            return configuration.ConnectionString;
        }

        var connection = configuration.ConnectionOptions
                         ?? throw new ArgumentException("Connection configuration is missing.");

        var providerConfig = serviceProvider
                .GetService(typeof(IConfigurationAccessor<MsSqlServerOptions>)) as
            IConfigurationAccessor<MsSqlServerOptions>;

        var settings = providerConfig?.Current;
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = connection.Host,
            InitialCatalog = connection.Name,
            UserID = connection.Username,
            Password = connection.Password,
            TrustServerCertificate = settings?.TrustServerCertificate ?? false,
            MultipleActiveResultSets = settings?.MultipleActiveResultSets ?? false
        };

        logger?.LogDebug(
            "Resolved SQL Server connection string from structured configuration. TrustServerCertificate: {TrustServerCertificate}, MultipleActiveResultSets: {MultipleActiveResultSets}, CommandTimeoutConfigured: {CommandTimeoutConfigured}.",
            builder.TrustServerCertificate,
            builder.MultipleActiveResultSets,
            settings?.CommandTimeoutSeconds is not null);

        return builder.ConnectionString;
    }

    /// <inheritdoc />
    public void Configure(
        DbContextOptionsBuilder options,
        string connectionString,
        DatabaseOptions configuration,
        Assembly migrationsAssembly,
        IServiceProvider serviceProvider)
    {
        var providerConfig = serviceProvider
                .GetService(typeof(IConfigurationAccessor<MsSqlServerOptions>)) as
            IConfigurationAccessor<MsSqlServerOptions>;

        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                configuration.RetryCount,
                TimeSpan.FromSeconds(configuration.RetryDelaySeconds),
                null);

            sqlOptions.MigrationsAssembly(migrationsAssembly.GetName().Name);

            var settings = providerConfig?.Current;
            if (settings?.CommandTimeoutSeconds is not null)
            {
                sqlOptions.CommandTimeout(settings.CommandTimeoutSeconds.Value);
            }
        });

        logger?.LogDebug(
            "Configured EF Core SQL Server provider. Migrations assembly: {MigrationsAssembly}, RetryCount: {RetryCount}, RetryDelaySeconds: {RetryDelaySeconds}.",
            migrationsAssembly.GetName().Name,
            configuration.RetryCount,
            configuration.RetryDelaySeconds);
    }

    /// <inheritdoc />
    public void Validate(DatabaseOptions configuration)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            logger?.LogDebug("Skipping structured SQL Server validation because a raw connection string is configured.");
            return;
        }

        var connection = configuration.ConnectionOptions
                         ?? throw new InvalidOperationException("SQL Server connection configuration is missing.");

        if (string.IsNullOrWhiteSpace(connection.Host))
        {
            throw new InvalidOperationException("SQL Server connection requires a host.");
        }

        if (string.IsNullOrWhiteSpace(connection.Name))
        {
            throw new InvalidOperationException("SQL Server connection requires a database name.");
        }

        logger?.LogDebug("SQL Server structured connection configuration validated.");
    }
}

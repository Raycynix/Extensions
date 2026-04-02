using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Configurations;
using Raycynix.Extensions.Database.Enums;

namespace Raycynix.Extensions.Database.Internal;

/// <summary>
/// Implements SQL Server-specific connection and EF Core configuration for the shared database context.
/// </summary>
internal sealed class MsSqlServerDatabaseProviderRegistration : IDatabaseProviderRegistration
{
    /// <inheritdoc />
    public DatabaseProvider Provider => DatabaseProvider.MsSqlServer;

    /// <inheritdoc />
    public string ResolveConnectionString(DatabaseConfiguration configuration, IServiceProvider serviceProvider)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ConnectionString))
        {
            return configuration.ConnectionString;
        }

        var connection = configuration.ConnectionConfiguration
                         ?? throw new ArgumentException("Connection configuration is missing.");

        var providerConfig = configuration.MsSqlServerConfiguration;
        var builder = new SqlConnectionStringBuilder
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

    /// <inheritdoc />
    public void Configure(
        DbContextOptionsBuilder options,
        string connectionString,
        DatabaseConfiguration configuration,
        Assembly migrationsAssembly,
        IServiceProvider serviceProvider)
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                configuration.RetryCount,
                TimeSpan.FromSeconds(configuration.RetryDelaySeconds),
                null);

            sqlOptions.MigrationsAssembly(migrationsAssembly.GetName().Name);

            var providerConfig = configuration.MsSqlServerConfiguration;
            if (providerConfig?.CommandTimeoutSeconds is not null)
            {
                sqlOptions.CommandTimeout(providerConfig.CommandTimeoutSeconds.Value);
            }
        });
    }
}

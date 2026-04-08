using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Configurations;
using Raycynix.Extensions.Database.MsSql.Configurations;

namespace Raycynix.Extensions.Database.MsSql.Internal;

/// <summary>
/// Implements SQL Server-specific connection and EF Core configuration for the shared database context.
/// </summary>
internal sealed class MsSqlServerDatabaseProviderRegistration : IDatabaseProviderRegistration
{
    /// <inheritdoc />
    public string ProviderName => "sqlserver";

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
                .GetService(typeof(IConfigurationAccessor<MsSqlServerConfiguration>)) as
            IConfigurationAccessor<MsSqlServerConfiguration>;

        var settings = providerConfig?.Current;
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = connection.Host,
            InitialCatalog = connection.Name,
            UserID = connection.Username,
            Password = connection.Password,
            TrustServerCertificate = settings?.TrustServerCertificate ?? true,
            MultipleActiveResultSets = settings?.MultipleActiveResultSets ?? false
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
                .GetService(typeof(IConfigurationAccessor<MsSqlServerConfiguration>)) as
            IConfigurationAccessor<MsSqlServerConfiguration>;

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
    }
}

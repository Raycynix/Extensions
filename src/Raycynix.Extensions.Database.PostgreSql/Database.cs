using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Configurations;
using Raycynix.Extensions.Database.Enums;
using Raycynix.Extensions.Database.PostgreSql.Configurations;
using Raycynix.Extensions.Database.PostgreSql.Internal;

namespace Raycynix.Extensions.Database.PostgreSql;

/// <summary>
/// Provides PostgreSQL-specific database registration extensions.
/// </summary>
public static class Database
{
    /// <summary>
    /// Adds PostgreSQL provider support to the shared Raycynix database registration.
    /// </summary>
    /// <param name="builder">The shared database builder.</param>
    /// <param name="configure">An optional callback for adjusting PostgreSQL-specific settings.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public static DatabaseBuilder AddPostgreSql(
        this DatabaseBuilder builder,
        Action<PostgreSqlConfiguration>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddRaycynixConfiguration<PostgreSqlConfiguration>(
            builder.Configuration,
            configurePostBind: configure);

        builder.Services.AddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IConfigurationAccessor<PostgreSqlConfiguration>>().Current);

        builder.Services.AddSingleton(serviceProvider =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfigurationAccessor<DatabaseConfiguration>>().Current;

            return new DatabaseConfiguration
            {
                ConnectionString = configuration.ConnectionString,
                ConnectionConfiguration = configuration.ConnectionConfiguration,
                Provider = DatabaseProvider.PostgreSql,
                UseMigrations = configuration.UseMigrations,
                EnsureCreated = configuration.EnsureCreated,
                EnableSeed = configuration.EnableSeed,
                EnableLazyLoading = configuration.EnableLazyLoading,
                EnableAutoDetectChanges = configuration.EnableAutoDetectChanges,
                UseQueryTrackingByDefault = configuration.UseQueryTrackingByDefault,
                RetryCount = configuration.RetryCount,
                RetryDelaySeconds = configuration.RetryDelaySeconds,
                MsSqlServerConfiguration = configuration.MsSqlServerConfiguration,
                MySqlConfiguration = configuration.MySqlConfiguration,
                SqlliteConfiguration = configuration.SqlliteConfiguration
            };
        });

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IDatabaseProviderRegistration, PostgreSqlDatabaseProviderRegistration>());

        return builder;
    }
}

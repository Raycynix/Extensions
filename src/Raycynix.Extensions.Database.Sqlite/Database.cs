using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Configurations;
using Raycynix.Extensions.Database.Sqlite.Configurations;
using Raycynix.Extensions.Database.Sqlite.Internal;

namespace Raycynix.Extensions.Database.Sqlite;

/// <summary>
/// Provides SQLite-specific database registration extensions.
/// </summary>
public static class Database
{
    /// <summary>
    /// Adds SQLite provider support to the shared Raycynix database registration.
    /// </summary>
    /// <param name="builder">The shared database builder.</param>
    /// <param name="configure">An optional callback for adjusting SQLite-specific settings.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public static IDatabaseBuilder AddSqlite(
        this IDatabaseBuilder builder,
        Action<SqliteConfiguration>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddRaycynixConfiguration<SqliteConfiguration>(
            builder.Configuration,
            $"{nameof(DatabaseConfiguration)}:{nameof(SqliteConfiguration)}",
            configurePostBind: configure);

        builder.Services.AddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IConfigurationAccessor<SqliteConfiguration>>().Current);

        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IDatabaseProviderRegistration, SqliteDatabaseProviderRegistration>());

        return builder;
    }
}

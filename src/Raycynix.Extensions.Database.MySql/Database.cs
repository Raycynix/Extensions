using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Configurations;
using Raycynix.Extensions.Database.MySql.Configurations;
using Raycynix.Extensions.Database.MySql.Internal;

namespace Raycynix.Extensions.Database.MySql;

/// <summary>
/// Provides MySQL-specific database registration extensions.
/// </summary>
public static class Database
{
    /// <summary>
    /// Adds MySQL provider support to the shared Raycynix database registration.
    /// </summary>
    /// <param name="builder">The shared database builder.</param>
    /// <param name="configure">An optional callback for adjusting MySQL-specific settings.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public static DatabaseBuilder AddMySql(
        this DatabaseBuilder builder,
        Action<MySqlConfiguration>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddRaycynixConfiguration<MySqlConfiguration>(
            builder.Configuration,
            $"{nameof(DatabaseConfiguration)}:{nameof(MySqlConfiguration)}",
            configurePostBind: configure);

        builder.Services.AddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IConfigurationAccessor<MySqlConfiguration>>().Current);

        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IDatabaseProviderRegistration, MySqlDatabaseProviderRegistration>());

        return builder;
    }
}

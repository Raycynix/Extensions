using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Options;
using Raycynix.Extensions.Database.MySql.Options;
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
    public static IDatabaseBuilder AddMySql(
        this IDatabaseBuilder builder,
        Action<MySqlOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddRaycynixConfiguration<MySqlOptions>(
            builder.Configuration,
            $"{nameof(DatabaseOptions)}:{nameof(MySqlOptions)}",
            configurePostBind: configure);
        builder.Services.AddRaycynixConfigurationValidator<MySqlOptions, MySqlOptionsValidator>();

        builder.Services.TryAddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IConfigurationAccessor<MySqlOptions>>().Current);

        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IDatabaseProviderRegistration, MySqlDatabaseProviderRegistration>());

        return builder;
    }
}

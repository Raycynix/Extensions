using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Configurations;
using Raycynix.Extensions.Database.MsSql.Configurations;
using Raycynix.Extensions.Database.MsSql.Internal;

namespace Raycynix.Extensions.Database.MsSql;

/// <summary>
/// Provides SQL Server-specific database registration extensions.
/// </summary>
public static class Database
{
    /// <summary>
    /// Adds SQL Server provider support to the shared Raycynix database registration.
    /// </summary>
    /// <param name="builder">The shared database builder.</param>
    /// <param name="configure">An optional callback for adjusting SQL Server-specific settings.</param>
    /// <returns>The same builder instance for chaining.</returns>
    public static DatabaseBuilder AddMsSql(
        this DatabaseBuilder builder,
        Action<MsSqlServerConfiguration>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddRaycynixConfiguration<MsSqlServerConfiguration>(
            builder.Configuration,
            $"{nameof(DatabaseConfiguration)}:{nameof(MsSqlServerConfiguration)}",
            configurePostBind: configure);

        builder.Services.AddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IConfigurationAccessor<MsSqlServerConfiguration>>().Current);

        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IDatabaseProviderRegistration, MsSqlServerDatabaseProviderRegistration>());

        return builder;
    }
}

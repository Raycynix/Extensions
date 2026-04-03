using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Configurations;
using Raycynix.Extensions.Database.Implementations;
using Raycynix.Extensions.Database.Internal;

namespace Raycynix.Extensions.Database;

/// <summary>
/// Provides service registration extensions for the Raycynix database package.
/// </summary>
public static class Database
{
    /// <param name="services">The service collection to update.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the database context, initializer, and shared database infrastructure.
        /// </summary>
        /// <param name="configuration">The application configuration used to bind <see cref="DatabaseConfiguration"/>.</param>
        /// <param name="setup">An optional callback for adjusting the bound database configuration.</param>
        /// <returns>A builder that can be used to extend the database registration.</returns>
        public DatabaseBuilder AddRaycynixDatabase(IConfiguration configuration,
            Action<DatabaseConfiguration>? setup = null)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            var callerAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
            var modelAssemblyRegistry = GetOrCreateModelAssemblyRegistry(services);
            modelAssemblyRegistry.Add(callerAssembly);

            services.AddRaycynixConfiguration<DatabaseConfiguration>(
                configuration,
                configurePostBind: setup);

            services.AddRaycynixConfigurationValidator<DatabaseConfiguration, DatabaseConfigurationValidator>();
            services.AddSingleton(serviceProvider =>
                serviceProvider.GetRequiredService<IConfigurationAccessor<DatabaseConfiguration>>().Current);

            services.TryAddSingleton(modelAssemblyRegistry);
            services.AddSingleton(static serviceProvider => ResolveProviderDescriptor(serviceProvider));

            services.AddSingleton<DatabaseObservability>();
            services.AddSingleton<IDatabaseInitializer, DatabaseInitializer>();

            services.AddDbContextPool<DatabaseContext>((serviceProvider, options) =>
            {
                var config = serviceProvider.GetRequiredService<DatabaseConfiguration>();
                var providerRegistration = serviceProvider.GetRequiredService<DatabaseProviderDescriptor>().Registration;

                var connectionString = providerRegistration.ResolveConnectionString(config, serviceProvider);
                options.ReplaceService<IModelCacheKeyFactory, DatabaseModelCacheKeyFactory>();
                providerRegistration.Configure(options, connectionString, config, callerAssembly, serviceProvider);
            });

            return new DatabaseBuilder(services, configuration, callerAssembly);
        }

        /// <summary>
        /// Registers an additional assembly that contributes EF Core configurators to the shared database context.
        /// </summary>
        /// <param name="assembly">The assembly to register.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public IServiceCollection AddRaycynixDatabaseAssembly(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(assembly);

            var modelAssemblyRegistry = GetOrCreateModelAssemblyRegistry(services);
            modelAssemblyRegistry.Add(assembly);
            return services;
        }

        /// <summary>
        /// Registers an additional assembly that contributes EF Core configurators to the shared database context.
        /// </summary>
        /// <typeparam name="TMarker">A marker type from the assembly to register.</typeparam>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public IServiceCollection AddRaycynixDatabaseAssembly<TMarker>()
        {
            return services.AddRaycynixDatabaseAssembly(typeof(TMarker).Assembly);
        }
    }

    private static DatabaseModelAssemblyRegistry GetOrCreateModelAssemblyRegistry(IServiceCollection services)
    {
        if (services
                .FirstOrDefault(static descriptor => descriptor.ServiceType == typeof(DatabaseModelAssemblyRegistry))
                ?.ImplementationInstance is DatabaseModelAssemblyRegistry existingRegistry)
        {
            return existingRegistry;
        }

        var registry = new DatabaseModelAssemblyRegistry();
        services.AddSingleton(registry);
        return registry;
    }

    private static DatabaseProviderDescriptor ResolveProviderDescriptor(IServiceProvider serviceProvider)
    {
        var registrations = serviceProvider.GetServices<IDatabaseProviderRegistration>().ToArray();

        return registrations.Length switch
        {
            1 => new DatabaseProviderDescriptor
            {
                ProviderName = registrations[0].ProviderName,
                Registration = registrations[0]
            },
            0 => throw new NotSupportedException(
                "No database provider is registered. Add exactly one matching provider package, for example AddSqlite(), AddPostgreSql(), AddMsSql(), or AddMySql()."),
            _ => throw new InvalidOperationException(
                $"Multiple database providers are registered ({string.Join(", ", registrations.Select(static registration => registration.ProviderName))}). Register exactly one database provider package.")
        };
    }
}

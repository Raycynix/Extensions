using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Database.Abstractions;
using Raycynix.Extensions.Database.Abstractions.Configurations;
using Raycynix.Extensions.Database.Implementations;
using Raycynix.Extensions.Database.Internal;
using Raycynix.Extensions.Logging;

namespace Raycynix.Extensions.Database;

/// <summary>
/// Provides service registration extensions for the Raycynix database package.
/// </summary>
public static class Database
{
    /// <summary>
    /// Extends service collections with Raycynix database registration APIs.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the Raycynix database infrastructure using separate marker types for model configurators and EF Core migrations.
        /// </summary>
        /// <typeparam name="TContext">The concrete Raycynix database context type to register.</typeparam>
        /// <typeparam name="TMarker">A marker type from the assembly that contributes EF Core configurators.</typeparam>
        /// <typeparam name="TMigrationMarker">A marker type from the assembly that contains EF Core migrations.</typeparam>
        /// <param name="configuration">The application configuration used to bind <see cref="DatabaseConfiguration"/>.</param>
        /// <param name="setup">An optional callback for adjusting the bound database configuration.</param>
        /// <returns>A builder that can be used to extend the database registration.</returns>
        public IDatabaseBuilder AddRaycynixDatabase<TContext, TMarker, TMigrationMarker>(
            IConfiguration configuration,
            Action<DatabaseConfiguration>? setup = null)
            where TContext : RaycynixDatabaseContext
        {
            var migrationsAssembly = typeof(TMigrationMarker).Assembly;
            var modelAssembly = typeof(TMarker).Assembly;

            return RegisterRaycynixDatabase<TContext>(
                services,
                configuration,
                migrationsAssembly,
                setup,
                modelAssembly);
        }

        /// <summary>
        /// Registers the Raycynix database infrastructure using a custom context type and a marker type
        /// from the assembly that contributes EF Core configurators and contains EF Core migrations.
        /// </summary>
        /// <typeparam name="TContext">The concrete Raycynix database context type to register.</typeparam>
        /// <typeparam name="TMarker">A marker type from the assembly to register for configurators and migrations.</typeparam>
        /// <param name="configuration">The application configuration used to bind <see cref="DatabaseConfiguration"/>.</param>
        /// <param name="setup">An optional callback for adjusting the bound database configuration.</param>
        /// <returns>A builder that can be used to extend the database registration.</returns>
        public IDatabaseBuilder AddRaycynixDatabase<TContext, TMarker>(
            IConfiguration configuration,
            Action<DatabaseConfiguration>? setup = null)
            where TContext : RaycynixDatabaseContext
        {
            var assembly = typeof(TMarker).Assembly;

            return RegisterRaycynixDatabase<TContext>(
                services,
                configuration,
                assembly,
                setup,
                assembly);
        }

        /// <summary>
        /// Registers the Raycynix database infrastructure using explicit model and migrations assemblies.
        /// </summary>
        /// <typeparam name="TContext">The concrete Raycynix database context type to register.</typeparam>
        /// <param name="configuration">The application configuration used to bind <see cref="DatabaseConfiguration"/>.</param>
        /// <param name="migrationsAssembly">The assembly that contains EF Core migrations.</param>
        /// <param name="modelAssembly">The assembly that contributes EF Core configurators.</param>
        /// <param name="setup">An optional callback for adjusting the bound database configuration.</param>
        /// <returns>A builder that can be used to extend the database registration.</returns>
        public IDatabaseBuilder AddRaycynixDatabase<TContext>(
            IConfiguration configuration,
            Assembly migrationsAssembly,
            Assembly modelAssembly,
            Action<DatabaseConfiguration>? setup = null)
            where TContext : RaycynixDatabaseContext
        {
            ArgumentNullException.ThrowIfNull(migrationsAssembly);
            ArgumentNullException.ThrowIfNull(modelAssembly);

            return RegisterRaycynixDatabase<TContext>(
                services,
                configuration,
                migrationsAssembly,
                setup,
                modelAssembly);
        }

        /// <summary>
        /// Registers the Raycynix database infrastructure using a custom context type and an explicit assembly
        /// that contributes EF Core configurators and contains EF Core migrations.
        /// </summary>
        /// <typeparam name="TContext">The concrete Raycynix database context type to register.</typeparam>
        /// <param name="configuration">The application configuration used to bind <see cref="DatabaseConfiguration"/>.</param>
        /// <param name="assembly">The assembly to register for configurators and migrations.</param>
        /// <param name="setup">An optional callback for adjusting the bound database configuration.</param>
        /// <returns>A builder that can be used to extend the database registration.</returns>
        public IDatabaseBuilder AddRaycynixDatabase<TContext>(
            IConfiguration configuration,
            Assembly assembly,
            Action<DatabaseConfiguration>? setup = null)
            where TContext : RaycynixDatabaseContext
        {
            ArgumentNullException.ThrowIfNull(assembly);

            return RegisterRaycynixDatabase<TContext>(
                services,
                configuration,
                assembly,
                setup,
                assembly);
        }

        /// <summary>
        /// Registers the Raycynix database infrastructure using a custom context type.
        /// </summary>
        /// <typeparam name="TContext">The concrete Raycynix database context type to register.</typeparam>
        /// <param name="configuration">The application configuration used to bind <see cref="DatabaseConfiguration"/>.</param>
        /// <param name="setup">An optional callback for adjusting the bound database configuration.</param>
        /// <param name="registerCallerAssembly">
        /// The entry or caller assembly is always used as the default EF Core migrations assembly.
        /// When this value is <see langword="true"/>, that assembly is also scanned for configurators.
        /// Disable this when assemblies should be registered explicitly.
        /// </param>
        /// <returns>A builder that can be used to extend the database registration.</returns>
        public IDatabaseBuilder AddRaycynixDatabase<TContext>(IConfiguration configuration,
            Action<DatabaseConfiguration>? setup = null,
            bool registerCallerAssembly = true) where TContext : RaycynixDatabaseContext
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            var callerAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();

            return RegisterRaycynixDatabase<TContext>(
                services,
                configuration,
                callerAssembly,
                setup,
                registerCallerAssembly ? callerAssembly : null);
        }

        /// <summary>
        /// Registers the Raycynix database infrastructure using the default <see cref="DatabaseContext"/>.
        /// </summary>
        /// <param name="configuration">The application configuration used to bind <see cref="DatabaseConfiguration"/>.</param>
        /// <param name="setup">An optional callback for adjusting the bound database configuration.</param>
        /// <param name="registerCallerAssembly">
        /// The entry or caller assembly is always used as the default EF Core migrations assembly.
        /// When this value is <see langword="true"/>, that assembly is also scanned for configurators.
        /// Disable this when assemblies should be registered explicitly.
        /// </param>
        /// <returns>A builder that can be used to extend the database registration.</returns>
        public IDatabaseBuilder AddRaycynixDatabase(IConfiguration configuration,
            Action<DatabaseConfiguration>? setup = null, bool registerCallerAssembly = true)
        {
            return services.AddRaycynixDatabase<DatabaseContext>(
                configuration,
                setup,
                registerCallerAssembly);
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

    private static DatabaseBuilder RegisterRaycynixDatabase<TContext>(
        IServiceCollection services,
        IConfiguration configuration,
        Assembly migrationsAssembly,
        Action<DatabaseConfiguration>? setup,
        Assembly? modelAssembly)
        where TContext : RaycynixDatabaseContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(migrationsAssembly);

        EnsureContextRegistrationIsCompatible<TContext>(services);

        var modelAssemblyRegistry = GetOrCreateModelAssemblyRegistry(services);
        if (modelAssembly is not null)
        {
            modelAssemblyRegistry.Add(modelAssembly);
        }

        if (IsContextRegistered<TContext>(services))
        {
            if (setup is not null)
            {
                throw new InvalidOperationException(
                    "Raycynix database is already registered. Configure DatabaseConfiguration only on the first AddRaycynixDatabase call.");
            }

            return new DatabaseBuilder(services, configuration, migrationsAssembly);
        }

        services.AddRaycynixLogging(configuration);

        services.AddRaycynixConfiguration<DatabaseConfiguration>(
            configuration,
            configurePostBind: setup);

        services.AddRaycynixConfigurationValidator<DatabaseConfiguration, DatabaseConfigurationValidator>();
        services.TryAddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IConfigurationAccessor<DatabaseConfiguration>>().Current);

        services.TryAddSingleton<IDatabaseModelAssemblyRegistry>(modelAssemblyRegistry);
        services.TryAddSingleton(static serviceProvider => ResolveProviderDescriptor(serviceProvider));

        services.TryAddSingleton<IDatabaseObservability, NoOpDatabaseObservability>();
        services.TryAddSingleton<IDatabaseInitializer, DatabaseInitializer>();

        if (services.All(static descriptor => descriptor.ServiceType != typeof(TContext)))
        {
            services.AddDbContext<TContext>((serviceProvider, options) =>
            {
                var config = serviceProvider.GetRequiredService<DatabaseConfiguration>();
                var providerRegistration =
                    serviceProvider.GetRequiredService<DatabaseProviderDescriptor>().Registration;

                providerRegistration.Validate(config);

                var connectionString = providerRegistration.ResolveConnectionString(config, serviceProvider);
                options.ReplaceService<IModelCacheKeyFactory, DatabaseModelCacheKeyFactory>();
                providerRegistration.Configure(options, connectionString, config, migrationsAssembly, serviceProvider);
            });
        }

        services.TryAddScoped<RaycynixDatabaseContext>(provider =>
            provider.GetRequiredService<TContext>());
        services.TryAddSingleton(new DatabaseContextDescriptor
        {
            ContextType = typeof(TContext)
        });

        return new DatabaseBuilder(services, configuration, migrationsAssembly);
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

    private static void EnsureContextRegistrationIsCompatible<TContext>(IServiceCollection services)
        where TContext : RaycynixDatabaseContext
    {
        var registeredContextType = GetRegisteredContextType(services);
        if (registeredContextType is null || registeredContextType == typeof(TContext))
        {
            return;
        }

        throw new InvalidOperationException(
            $"Raycynix database is already registered with context type {registeredContextType.FullName}. " +
            $"It cannot be registered again with context type {typeof(TContext).FullName}.");
    }

    private static bool IsContextRegistered<TContext>(IServiceCollection services)
        where TContext : RaycynixDatabaseContext
    {
        return GetRegisteredContextType(services) == typeof(TContext);
    }

    private static Type? GetRegisteredContextType(IServiceCollection services)
    {
        return services
            .FirstOrDefault(static descriptor => descriptor.ServiceType == typeof(DatabaseContextDescriptor))
            ?.ImplementationInstance is DatabaseContextDescriptor descriptor
            ? descriptor.ContextType
            : null;
    }
}

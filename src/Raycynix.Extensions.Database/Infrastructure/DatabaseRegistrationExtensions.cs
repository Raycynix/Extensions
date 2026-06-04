using System.Reflection;
using Microsoft.EntityFrameworkCore;
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

namespace Raycynix.Extensions.Database.Infrastructure;

public class DatabaseRegistrationExtensions
{
    public static IDatabaseBuilder RegisterRaycynixDatabaseCore<TContext>(
        IServiceCollection services,
        IConfiguration configuration,
        Assembly migrationsAssembly,
        Action<DatabaseConfiguration>? setup,
        Assembly? modelAssembly)
        where TContext : DbContext, IRaycynixDatabaseContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(migrationsAssembly);

        EnsureContextRegistrationIsCompatible<TContext>(services);

        var modelAssemblyRegistry = DatabaseModelAssemblyRegistry.GetOrCreate(services);
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
        services.TryAddSingleton(static serviceProvider =>
            DatabaseProviderDescriptor.Resolve(serviceProvider));

        services.TryAddScoped<IDatabaseModelConfigurator, DatabaseModelConfigurator>();
        services.TryAddSingleton<IDatabaseObservability, NoOpDatabaseObservability>();
        services.TryAddSingleton<IDatabaseInitializer, DatabaseInitializer<TContext>>();

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

        services.TryAddScoped<IRaycynixDatabaseContext>(provider =>
            provider.GetRequiredService<TContext>());

        services.TryAddSingleton(new DatabaseContextDescriptor
        {
            ContextType = typeof(TContext)
        });

        return new DatabaseBuilder(services, configuration, migrationsAssembly);
    }

    private static void EnsureContextRegistrationIsCompatible<TContext>(IServiceCollection services)
        where TContext : IRaycynixDatabaseContext
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
        where TContext : IRaycynixDatabaseContext
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
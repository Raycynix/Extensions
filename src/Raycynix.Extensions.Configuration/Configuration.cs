using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.Abstractions.Models;
using Raycynix.Extensions.Configuration.Configurations;
using Raycynix.Extensions.Configuration.Implementations;
using Raycynix.Extensions.Configuration.Internal;

namespace Raycynix.Extensions.Configuration;

/// <summary>
/// Provides service registration extensions for the Raycynix configuration package.
/// </summary>
public static class Configuration
{
    /// <summary>
    /// Registers the standard Raycynix application environment abstraction.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="environmentName">The current environment name.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixEnvironment(
        this IServiceCollection services,
        string environmentName)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (string.IsNullOrWhiteSpace(environmentName))
        {
            throw new ArgumentException("Environment name cannot be null or whitespace.", nameof(environmentName));
        }

        services.TryAddSingleton<IApplicationEnvironment>(_ => new ApplicationEnvironment(environmentName));

        return services;
    }

    /// <summary>
    /// Registers the standard Raycynix application environment abstraction from the host environment.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixEnvironment(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IApplicationEnvironment>(serviceProvider =>
        {
            var hostEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();
            return new ApplicationEnvironment(hostEnvironment.EnvironmentName);
        });

        return services;
    }

    /// <summary>
    /// Adds the standard Raycynix configuration sources to the provided builder.
    /// </summary>
    /// <param name="builder">The configuration builder to update.</param>
    /// <param name="setup">An optional callback for adjusting source registration behavior.</param>
    /// <returns>The same <see cref="IConfigurationBuilder"/> instance for chaining.</returns>
    public static IConfigurationBuilder AddRaycynixConfigurationSources(
        this IConfigurationBuilder builder,
        Action<ConfigurationSourcesConfiguration>? setup = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var config = new ConfigurationSourcesConfiguration();
        setup?.Invoke(config);

        ValidateSourcesConfiguration(config);
        RegisterSources(builder, config);

        return builder;
    }

    /// <summary>
    /// Replaces the current configuration sources with the standard Raycynix source order.
    /// </summary>
    /// <param name="builder">The configuration builder to update.</param>
    /// <param name="setup">An optional callback for adjusting source registration behavior.</param>
    /// <returns>The same <see cref="IConfigurationBuilder"/> instance for chaining.</returns>
    public static IConfigurationBuilder UseRaycynixConfigurationSources(
        this IConfigurationBuilder builder,
        Action<ConfigurationSourcesConfiguration>? setup = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Sources.Clear();
        return builder.AddRaycynixConfigurationSources(setup);
    }

    /// <summary>
    /// Registers the standard Raycynix feature flags configuration and accessor.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="configuration">The application configuration source.</param>
    /// <param name="sectionName">An optional feature flags section name. Defaults to <c>FeatureFlags</c>.</param>
    /// <param name="configureDefaults">An optional callback for applying default feature flag values before binding.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixFeatureFlags(
        this IServiceCollection services,
        IConfiguration configuration,
        string? sectionName = null,
        Action<FeatureFlagsConfiguration>? configureDefaults = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        sectionName ??= "FeatureFlags";

        services.AddRaycynixConfiguration(configuration, sectionName, configureDefaults);
        services.TryAddSingleton<IFeatureFlagAccessor, FeatureFlagAccessor>();

        return services;
    }

    /// <summary>
    /// Registers a typed configuration model using the standard Options pipeline with Raycynix conventions.
    /// </summary>
    /// <typeparam name="TOptions">The configuration model type.</typeparam>
    /// <param name="services">The service collection to update.</param>
    /// <param name="configuration">The application configuration source.</param>
    /// <param name="sectionName">An optional configuration section name. Defaults to the model type name.</param>
    /// <param name="configureDefaults">An optional callback for applying default values before configuration binding.</param>
    /// <param name="configureBinder">An optional callback for binder behavior customization.</param>
    /// <param name="configurePostBind">An optional callback for adjusting the bound options before validation and access.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixConfiguration<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string? sectionName = null,
        Action<TOptions>? configureDefaults = null,
        Action<BinderOptions>? configureBinder = null,
        Action<TOptions>? configurePostBind = null)
        where TOptions : class, new()
    {
        sectionName ??= typeof(TOptions).Name;
        var section = configuration.GetSection(sectionName);

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IConfigurationDefaults<TOptions>, EmptyConfigurationDefaults<TOptions>>());
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<TOptions>, RaycynixValidateOptions<TOptions>>());
        services.TryAddSingleton<ConfigurationRuntimeState<TOptions>>();
        services.TryAddSingleton<IConfigurationAccessor<TOptions>, ConfigurationAccessor<TOptions>>();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IConfigurationReloadPolicy<TOptions>, AttributeConfigurationReloadPolicy<TOptions>>());
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IConfigurationReloadPolicy<TOptions>, AllowConfigurationReloadPolicy<TOptions>>());
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IHostedService, ConfigurationChangeHostedService<TOptions>>());

        var optionsBuilder = services.AddOptions<TOptions>();

        optionsBuilder.Configure<IEnumerable<IConfigurationDefaults<TOptions>>>((options, defaultsProviders) =>
        {
            foreach (var defaultsProvider in defaultsProviders)
            {
                defaultsProvider.Apply(options);
            }

            configureDefaults?.Invoke(options);
        });

        optionsBuilder.Bind(section, configureBinder ?? (_ => { }));
        optionsBuilder.PostConfigure(options => configurePostBind?.Invoke(options));
        optionsBuilder.ValidateOnStart();

        return services;
    }

    /// <summary>
    /// Registers the unified typed configuration accessor for the specified configuration model.
    /// </summary>
    /// <typeparam name="TOptions">The configuration model type.</typeparam>
    /// <param name="services">The service collection to update.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixConfigurationAccessor<TOptions>(this IServiceCollection services)
        where TOptions : class, new()
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<ConfigurationRuntimeState<TOptions>>();
        services.TryAddSingleton<IConfigurationAccessor<TOptions>, ConfigurationAccessor<TOptions>>();

        return services;
    }

    /// <summary>
    /// Registers a Raycynix validator for a typed configuration model.
    /// </summary>
    /// <typeparam name="TOptions">The configuration model type.</typeparam>
    /// <typeparam name="TValidator">The validator type.</typeparam>
    /// <param name="services">The service collection to update.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixConfigurationValidator<TOptions, TValidator>(
        this IServiceCollection services)
        where TOptions : class, new()
        where TValidator : class, IConfigurationValidator<TOptions>
    {
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<TOptions>, RaycynixValidateOptions<TOptions>>());
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IConfigurationValidator<TOptions>, TValidator>());

        return services;
    }

    /// <summary>
    /// Registers an inline Raycynix validator for a typed configuration model.
    /// </summary>
    /// <typeparam name="TOptions">The configuration model type.</typeparam>
    /// <param name="services">The service collection to update.</param>
    /// <param name="validate">The validation delegate.</param>
    /// <param name="failureMessage">The error message returned when validation fails.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixConfigurationValidator<TOptions>(
        this IServiceCollection services,
        Func<TOptions, bool> validate,
        string failureMessage)
        where TOptions : class, new()
    {
        ArgumentNullException.ThrowIfNull(validate);

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<TOptions>, RaycynixValidateOptions<TOptions>>());
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IConfigurationValidator<TOptions>>(
                new DelegateConfigurationValidator<TOptions>(validate, failureMessage)));

        return services;
    }

    /// <summary>
    /// Registers a typed configuration change handler notified through the standard options monitor pipeline.
    /// </summary>
    /// <typeparam name="TOptions">The configuration model type.</typeparam>
    /// <typeparam name="THandler">The change handler type.</typeparam>
    /// <param name="services">The service collection to update.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixConfigurationChangeHandler<TOptions, THandler>(
        this IServiceCollection services)
        where TOptions : class, new()
        where THandler : class, IConfigurationChangeHandler<TOptions>
    {
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IConfigurationChangeHandler<TOptions>, THandler>());

        return services;
    }

    /// <summary>
    /// Registers an inline-typed configuration change handler notified through the standard options monitor pipeline.
    /// </summary>
    /// <typeparam name="TOptions">The configuration model type.</typeparam>
    /// <param name="services">The service collection to update.</param>
    /// <param name="handleAsync">The delegate to execute when the configuration changes.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixConfigurationChangeHandler<TOptions>(
        this IServiceCollection services,
        Func<ConfigurationChangeContext<TOptions>, CancellationToken, ValueTask> handleAsync)
        where TOptions : class, new()
    {
        ArgumentNullException.ThrowIfNull(handleAsync);

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IConfigurationChangeHandler<TOptions>>(
                new DelegateConfigurationChangeHandler<TOptions>(handleAsync)));

        return services;
    }

    /// <summary>
    /// Registers a reload policy for a typed configuration model.
    /// </summary>
    /// <typeparam name="TOptions">The configuration model type.</typeparam>
    /// <typeparam name="TReloadPolicy">The reload policy type.</typeparam>
    /// <param name="services">The service collection to update.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixConfigurationReloadPolicy<TOptions, TReloadPolicy>(
        this IServiceCollection services)
        where TOptions : class, new()
        where TReloadPolicy : class, IConfigurationReloadPolicy<TOptions>
    {
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IConfigurationReloadPolicy<TOptions>, TReloadPolicy>());

        return services;
    }

    /// <summary>
    /// Registers an inline reload policy for a typed configuration model.
    /// </summary>
    /// <typeparam name="TOptions">The configuration model type.</typeparam>
    /// <param name="services">The service collection to update.</param>
    /// <param name="evaluate">The reload policy delegate.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixConfigurationReloadPolicy<TOptions>(
        this IServiceCollection services,
        Func<ConfigurationChangeContext<TOptions>, ConfigurationReloadResult> evaluate)
        where TOptions : class, new()
    {
        ArgumentNullException.ThrowIfNull(evaluate);

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IConfigurationReloadPolicy<TOptions>>(
                new DelegateConfigurationReloadPolicy<TOptions>(evaluate)));

        return services;
    }

    private static void RegisterSources(IConfigurationBuilder builder, ConfigurationSourcesConfiguration config)
    {
        builder.SetBasePath(config.BasePath);
        builder.AddJsonFile(GetBaseJsonFileName(config.BaseFileName), config.BaseJsonOptional, config.ReloadOnChange);
        builder.AddJsonFile(
            GetEnvironmentSpecificFileName(config.BaseFileName, config.EnvironmentName),
            optional: true,
            reloadOnChange: config.ReloadOnChange);

        if (config.IncludeUserSecrets)
        {
            builder.AddUserSecrets(
                config.UserSecretsAssembly ?? Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly(),
                config.UserSecretsOptional,
                config.ReloadOnChange);
        }

        builder.AddEnvironmentVariables();

        if (config.CommandLineArguments.Length > 0)
        {
            builder.AddCommandLine(config.CommandLineArguments);
        }
    }

    private static void ValidateSourcesConfiguration(ConfigurationSourcesConfiguration config)
    {
        if (string.IsNullOrWhiteSpace(config.BasePath))
        {
            throw new ArgumentException("Configuration base path cannot be null or whitespace.", nameof(config));
        }

        if (string.IsNullOrWhiteSpace(config.EnvironmentName))
        {
            throw new ArgumentException("Environment name cannot be null or whitespace.", nameof(config));
        }

        if (string.IsNullOrWhiteSpace(config.BaseFileName))
        {
            throw new ArgumentException("Base configuration file name cannot be null or whitespace.", nameof(config));
        }
    }

    private static string GetBaseJsonFileName(string baseFileName)
    {
        return $"{baseFileName}.json";
    }

    private static string GetEnvironmentSpecificFileName(string baseFileName, string environmentName)
    {
        return $"{baseFileName}.{environmentName}.json";
    }
}

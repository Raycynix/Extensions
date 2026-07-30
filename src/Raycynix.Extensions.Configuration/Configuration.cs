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
    /// Extends configuration builders with Raycynix source registration APIs.
    /// </summary>
    /// <param name="builder">The configuration builder to update.</param>
    extension(IConfigurationBuilder builder)
    {
        /// <summary>
        /// Adds the standard Raycynix configuration sources to the provided builder.
        /// </summary>
        /// <param name="setup">An optional callback for adjusting source registration behavior.</param>
        /// <returns>The same <see cref="IConfigurationBuilder"/> instance for chaining.</returns>
        public IConfigurationBuilder AddRaycynixConfigurationSources(
            Action<ConfigurationSourcesOptions>? setup = null)
        {
            ArgumentNullException.ThrowIfNull(builder);

            var config = new ConfigurationSourcesOptions();
            setup?.Invoke(config);

            ValidateSourcesConfiguration(config);
            RegisterSources(builder, config);

            return builder;
        }

        /// <summary>
        /// Replaces the current configuration sources with the standard Raycynix source order.
        /// </summary>
        /// <param name="setup">An optional callback for adjusting source registration behavior.</param>
        /// <returns>The same <see cref="IConfigurationBuilder"/> instance for chaining.</returns>
        public IConfigurationBuilder UseRaycynixConfigurationSources(
            Action<ConfigurationSourcesOptions>? setup = null)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.Sources.Clear();
            return builder.AddRaycynixConfigurationSources(setup);
        }

        /// <summary>
        /// Adds a dotenv file as a configuration source.
        /// Double underscores in keys are mapped to configuration section delimiters.
        /// </summary>
        /// <param name="path">The dotenv file path relative to the configuration base path.</param>
        /// <param name="optional">Whether the file is optional.</param>
        /// <param name="reloadOnChange">Whether the configuration should reload when the file changes.</param>
        /// <returns>The same <see cref="IConfigurationBuilder"/> instance for chaining.</returns>
        public IConfigurationBuilder AddEnvFile(
            string path = ".env",
            bool optional = true,
            bool reloadOnChange = false)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            return builder.Add<EnvFileConfigurationSource>(source =>
            {
                source.Path = path;
                source.Optional = optional;
                source.ReloadOnChange = reloadOnChange;
            });
        }
    }

    /// <summary>
    /// Extends service collections with Raycynix typed configuration registration APIs.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the standard Raycynix application environment abstraction.
        /// </summary>
        /// <param name="environmentName">The current environment name.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public IServiceCollection AddRaycynixEnvironment(
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
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public IServiceCollection AddRaycynixEnvironment()
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
        /// Registers the standard Raycynix feature flags configuration and accessor.
        /// </summary>
        /// <param name="configuration">The application configuration source.</param>
        /// <param name="sectionName">An optional feature flags section name. Defaults to <c>FeatureFlags</c>.</param>
        /// <param name="configureDefaults">An optional callback for applying default feature flag values before binding.</param>
        /// <param name="requireSection">When <see langword="true"/>, registration fails if the feature flags section is missing.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public IServiceCollection AddRaycynixFeatureFlags(
            IConfiguration configuration,
            string? sectionName = null,
            Action<FeatureFlagsConfiguration>? configureDefaults = null,
            bool requireSection = false)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            var resolvedSectionName = sectionName ?? "FeatureFlags";
            var section = configuration.GetSection(resolvedSectionName);

            if (requireSection && !section.Exists())
            {
                throw new InvalidOperationException(
                    $"Required configuration feature flag section '{resolvedSectionName}' was not found.");
            }

            services.AddRaycynixConfiguration(
                configuration,
                sectionName: resolvedSectionName,
                configureDefaults: configureDefaults,
                requireSection: requireSection);
            services.TryAddSingleton<IFeatureFlagAccessor, FeatureFlagAccessor>();

            return services;
        }

        /// <summary>
        /// Registers a typed configuration model using the standard Options pipeline with Raycynix conventions.
        /// </summary>
        /// <typeparam name="TOptions">The configuration model type.</typeparam>
        /// <param name="configuration">The application configuration source.</param>
        /// <param name="sectionName">An optional configuration section name. Defaults to the model type name.</param>
        /// <param name="optionsName">An optional named options instance. Defaults to the standard Options default name.</param>
        /// <param name="configureDefaults">An optional callback for applying default values before configuration binding.</param>
        /// <param name="configureBinder">An optional callback for binder behavior customization.</param>
        /// <param name="configurePostBind">An optional callback for adjusting the bound options before validation and access.</param>
        /// <param name="requireSection">When <see langword="true"/>, registration fails if the configuration section is missing.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public IServiceCollection AddRaycynixConfiguration<TOptions>(
            IConfiguration configuration,
            string? sectionName = null,
            string? optionsName = null,
            Action<TOptions>? configureDefaults = null,
            Action<BinderOptions>? configureBinder = null,
            Action<TOptions>? configurePostBind = null,
            bool requireSection = false)
            where TOptions : class, new()
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            var resolvedSectionName = sectionName ?? ConfigurationSectionPath.For<TOptions>();
            var section = configuration.GetSection(resolvedSectionName);

            if (requireSection && !section.Exists())
            {
                throw new InvalidOperationException(
                    $"Required configuration section '{resolvedSectionName}' for options type '{typeof(TOptions).FullName}' was not found.");
            }

            services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IConfigurationDefaults<TOptions>, EmptyConfigurationDefaults<TOptions>>());
            services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IValidateOptions<TOptions>, RaycynixValidateOptions<TOptions>>());
            services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IConfigurationReloadPolicy<TOptions>, AttributeConfigurationReloadPolicy<TOptions>>());
            services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IConfigurationReloadPolicy<TOptions>, AllowConfigurationReloadPolicy<TOptions>>());
            services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IHostedService, ConfigurationChangeHostedService<TOptions>>());

            services.TryAddSingleton<ConfigurationRuntimeState<TOptions>>();
            services.TryAddSingleton<IConfigurationAccessor<TOptions>, ConfigurationAccessor<TOptions>>();
            services.TryAddSingleton<ConfigurationDiagnosticsOptions>();
            services.TryAddSingleton<IConfigurationRedactor, DefaultConfigurationRedactor>();
            services.TryAddSingleton<ConfigurationDiagnosticsStore>();
            services.TryAddSingleton<IConfigurationDiagnostics, ConfigurationDiagnostics>();

            var resolvedOptionsName = string.IsNullOrWhiteSpace(optionsName) ? Options.DefaultName : optionsName;
            services.AddSingleton(new ConfigurationOptionsRegistration<TOptions>(resolvedOptionsName,
                resolvedSectionName, requireSection));

            var optionsBuilder = services.AddOptions<TOptions>(resolvedOptionsName);

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
        /// Configures runtime diagnostics for Raycynix typed configuration registrations.
        /// </summary>
        /// <param name="configure">The diagnostics options callback.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public IServiceCollection ConfigureRaycynixConfigurationDiagnostics(
            Action<ConfigurationDiagnosticsOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configure);

            var options = new ConfigurationDiagnosticsOptions();
            configure(options);

            services.Replace(ServiceDescriptor.Singleton(options));

            return services;
        }

        /// <summary>
        /// Replaces the default configuration redactor with a custom implementation.
        /// </summary>
        /// <typeparam name="TRedactor">The redactor implementation type.</typeparam>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public IServiceCollection AddRaycynixConfigurationRedactor<TRedactor>()
            where TRedactor : class, IConfigurationRedactor
        {
            ArgumentNullException.ThrowIfNull(services);

            services.Replace(ServiceDescriptor.Singleton<IConfigurationRedactor, TRedactor>());

            return services;
        }

        /// <summary>
        /// Replaces the default configuration redactor with an inline delegate.
        /// </summary>
        /// <param name="redact">The redaction delegate.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public IServiceCollection AddRaycynixConfigurationRedactor(
            Func<string, object?, object?> redact)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(redact);

            services.Replace(ServiceDescriptor.Singleton<IConfigurationRedactor>(
                new DelegateConfigurationRedactor(redact)));

            return services;
        }

        /// <summary>
        /// Registers the unified typed configuration accessor for the specified configuration model.
        /// </summary>
        /// <typeparam name="TOptions">The configuration model type.</typeparam>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public IServiceCollection AddRaycynixConfigurationAccessor<TOptions>()
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
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public IServiceCollection AddRaycynixConfigurationValidator<TOptions, TValidator>()
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
        /// <param name="validate">The validation delegate.</param>
        /// <param name="failureMessage">The error message returned when validation fails.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public IServiceCollection AddRaycynixConfigurationValidator<TOptions>(Func<TOptions, bool> validate,
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
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public IServiceCollection AddRaycynixConfigurationChangeHandler<TOptions, THandler>()
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
        /// <param name="handleAsync">The delegate to execute when the configuration changes.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public IServiceCollection AddRaycynixConfigurationChangeHandler<TOptions>(
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
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public IServiceCollection AddRaycynixConfigurationReloadPolicy<TOptions, TReloadPolicy>()
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
        /// <param name="evaluate">The reload policy delegate.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
        public IServiceCollection AddRaycynixConfigurationReloadPolicy<TOptions>(
            Func<ConfigurationChangeContext<TOptions>, ConfigurationReloadResult> evaluate)
            where TOptions : class, new()
        {
            ArgumentNullException.ThrowIfNull(evaluate);

            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IConfigurationReloadPolicy<TOptions>>(
                    new DelegateConfigurationReloadPolicy<TOptions>(evaluate)));

            return services;
        }
    }

    private static void RegisterSources(IConfigurationBuilder builder, ConfigurationSourcesOptions config)
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

        if (config.IncludeEnvFile)
        {
            builder.AddEnvFile(config.EnvFileName, config.EnvFileOptional, config.ReloadOnChange);
        }

        builder.AddEnvironmentVariables();

        if (config.CommandLineArguments.Length > 0)
        {
            builder.AddCommandLine(config.CommandLineArguments);
        }
    }

    private static void ValidateSourcesConfiguration(ConfigurationSourcesOptions config)
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

        if (config.IncludeEnvFile && string.IsNullOrWhiteSpace(config.EnvFileName))
        {
            throw new ArgumentException("Environment file name cannot be null or whitespace.", nameof(config));
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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Logging.Abstractions;
using Raycynix.Extensions.Logging.Abstractions.Options;
using Raycynix.Extensions.Logging.Implementations;
using Raycynix.Extensions.Logging.Internal;
using Serilog;

namespace Raycynix.Extensions.Logging;

/// <summary>
/// Provides service registration and host configuration extensions for Raycynix logging.
/// </summary>
public static class Logging
{
    /// <summary>
    /// Provides extension methods for registering Raycynix logging services.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the Raycynix typed logger abstraction with the default logging configuration.
        /// </summary>
        /// <returns>A builder that can be used to register optional logging integrations.</returns>
        public LoggingBuilder AddRaycynixLogging()
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.TryAddSingleton(new LoggingOptions());

            return new LoggingBuilder(services);
        }
        
        /// <summary>
        /// Registers the Raycynix typed logger abstraction and the base logging configuration model.
        /// </summary>
        /// <param name="configuration">The application configuration source.</param>
        /// <param name="setup">An optional callback for adjusting the bound logging configuration.</param>
        /// <returns>A builder that can be used to register optional logging integrations.</returns>
        public LoggingBuilder AddRaycynixLogging(IConfiguration configuration,
            Action<LoggingOptions>? setup = null)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services.AddRaycynixConfiguration<LoggingOptions>(configuration, configurePostBind: setup);
            services.AddRaycynixConfigurationValidator<LoggingOptions, LoggingOptionsValidator>();
            services.AddSingleton(serviceProvider =>
                serviceProvider.GetRequiredService<IConfigurationAccessor<LoggingOptions>>().Current);

            services.TryAddSingleton(typeof(ILogger<>), typeof(Logger<>));

            return new LoggingBuilder(services, configuration);
        }
    }

    /// <summary>
    /// Configures Serilog using the <c>LoggingOptions</c> section and optional runtime overrides.
    /// </summary>
    /// <param name="hostBuilder">The host builder to configure.</param>
    /// <param name="setup">An optional callback for adjusting logging settings.</param>
    /// <returns>The configured <see cref="IHostBuilder"/> instance.</returns>
    public static IHostBuilder UseRaycynixLogging(
        this IHostBuilder hostBuilder,
        Action<LoggingOptions>? setup = null)
    {
        return hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
            ConfigureLogger(context, services, loggerConfiguration, setup));
    }

    private static void ConfigureLogger(
        HostBuilderContext context,
        IServiceProvider services,
        LoggerConfiguration loggerConfiguration,
        Action<LoggingOptions>? setup = null)
    {
        // Resolving options through DI here creates a cycle with the Serilog logging provider
        // while the host service provider is still being built.
        var options = context.Configuration.GetSection(nameof(LoggingOptions)).Get<LoggingOptions>()
                      ?? new LoggingOptions();

        if (string.IsNullOrWhiteSpace(options.Environment))
        {
            options.Environment = context.HostingEnvironment.EnvironmentName;
        }

        setup?.Invoke(options);
        options.Validate();

        loggerConfiguration
            .MinimumLevel.Is(LogLevelMapper.ToSerilog(options.MinimumLevel))
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", options.ServiceName)
            .Enrich.WithProperty("ServiceVersion", options.ServiceVersion)
            .Enrich.WithProperty("Environment", options.Environment)
            .WriteTo.Console(outputTemplate: options.OutputTemplate);

        foreach (var configurator in services.GetServices<IRaycynixLoggingConfigurator>())
        {
            configurator.Configure(context, services, loggerConfiguration, options);
        }
    }
}

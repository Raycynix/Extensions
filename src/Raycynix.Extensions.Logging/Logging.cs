using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Logging.Abstractions;
using Raycynix.Extensions.Logging.Abstractions.Configurations;
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
            services.TryAddSingleton(new LoggingConfiguration());

            return new LoggingBuilder(services);
        }
        
        /// <summary>
        /// Registers the Raycynix typed logger abstraction and the base logging configuration model.
        /// </summary>
        /// <param name="configuration">The application configuration source.</param>
        /// <param name="setup">An optional callback for adjusting the bound logging configuration.</param>
        /// <returns>A builder that can be used to register optional logging integrations.</returns>
        public LoggingBuilder AddRaycynixLogging(IConfiguration configuration,
            Action<LoggingConfiguration>? setup = null)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services.AddRaycynixConfiguration<LoggingConfiguration>(configuration, configurePostBind: setup);
            services.AddRaycynixConfigurationValidator<LoggingConfiguration, LoggingConfigurationValidator>();
            services.AddSingleton(serviceProvider =>
                serviceProvider.GetRequiredService<IConfigurationAccessor<LoggingConfiguration>>().Current);

            services.TryAddSingleton(typeof(ILogger<>), typeof(Logger<>));

            return new LoggingBuilder(services, configuration);
        }
    }

    /// <summary>
    /// Configures Serilog using the <c>LoggingConfiguration</c> section and optional runtime overrides.
    /// </summary>
    /// <param name="hostBuilder">The host builder to configure.</param>
    /// <param name="setup">An optional callback for adjusting logging settings.</param>
    /// <returns>The configured <see cref="IHostBuilder"/> instance.</returns>
    public static IHostBuilder UseRaycynixLogging(
        this IHostBuilder hostBuilder,
        Action<LoggingConfiguration>? setup = null)
    {
        return hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
        {
            var config = services.GetService<LoggingConfiguration>()
                         ?? context.Configuration.GetSection(nameof(LoggingConfiguration)).Get<LoggingConfiguration>()
                         ?? new LoggingConfiguration();

            if (string.IsNullOrWhiteSpace(config.Environment))
            {
                config.Environment = context.HostingEnvironment.EnvironmentName;
            }

            setup?.Invoke(config);
            config.Validate();

            loggerConfiguration
                .MinimumLevel.Is(LogLevelMapper.ToSerilog(config.MinimumLevel))
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ServiceName", config.ServiceName)
                .Enrich.WithProperty("ServiceVersion", config.ServiceVersion)
                .Enrich.WithProperty("Environment", config.Environment)
                .WriteTo.Console(outputTemplate: config.OutputTemplate);

            foreach (var configurator in services.GetServices<IRaycynixLoggingConfigurator>())
            {
                configurator.Configure(context, services, loggerConfiguration, config);
            }
        });
    }
}

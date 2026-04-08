using Elastic.CommonSchema.Serilog;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Logging.Abstractions;
using Raycynix.Extensions.Logging.Configurations;
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
    /// Registers the Raycynix typed logger abstraction.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixLogging(this IServiceCollection services)
    {
        services.TryAddSingleton(typeof(ILogger<>), typeof(Logger<>));
        return services;
    }

    /// <summary>
    /// Registers the Raycynix typed logger abstraction together with the typed logging configuration model.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="configuration">The application configuration source.</param>
    /// <param name="setup">An optional callback for adjusting the bound logging configuration.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRaycynixLogging(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<LoggingConfiguration>? setup = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddRaycynixConfiguration<LoggingConfiguration>(configuration, configurePostBind: setup);
        services.AddRaycynixConfigurationValidator<LoggingConfiguration, LoggingConfigurationValidator>();
        services.AddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IConfigurationAccessor<LoggingConfiguration>>().Current);

        return services.AddRaycynixLogging();
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
        return hostBuilder.UseSerilog((context, _, loggerConfiguration) =>
        {
            var config = context.Configuration.GetSection(nameof(LoggingConfiguration)).Get<LoggingConfiguration>() ??
                         new LoggingConfiguration();

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

            if (config.UseElastic)
            {
                loggerConfiguration.WriteTo.Elasticsearch([new Uri(config.ElasticUrl)], options =>
                {
                    options.DataStream = new DataStreamName(
                        "logs",
                        config.ServiceName.ToLowerInvariant(),
                        config.Environment.ToLowerInvariant()
                    );
                    options.TextFormatting = new EcsTextFormatterConfiguration<LogEventEcsDocument>();
                });
            }
        });
    }
}

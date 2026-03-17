using Elastic.CommonSchema.Serilog;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Common.Helpers;
using Raycynix.Extensions.Logging.Abstractions;
using Raycynix.Extensions.Logging.Configurations;
using Raycynix.Extensions.Logging.Implementation;
using Raycynix.Extensions.Logging.Internal;
using Serilog;

namespace Raycynix.Extensions.Logging;

/// <summary>
/// Provides service registration and host configuration extensions for Raycynix logging.
/// </summary>
public static class Logging
{
    /// <summary>
    /// Registers the Raycynix logger abstraction.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    public static IServiceCollection AddRaycynixLogging(this IServiceCollection services)
    {
        services.TryAddSingleton(typeof(ILogger<>), typeof(Logger<>));
        return services;
    }
    
    /// <summary>
    /// Configures Serilog using the Raycynix logging settings.
    /// </summary>
    /// <param name="hostBuilder">The host builder to configure.</param>
    /// <param name="setup">An optional callback for adjusting logging settings.</param>
    public static IHostBuilder UseRaycynixLogging(
        this IHostBuilder hostBuilder,
        Action<LoggingConfiguration>? setup = null)
    {
        return hostBuilder.UseSerilog((context, _, loggerConfiguration) =>
        {
            var config = context.Configuration.GetSection(nameof(LoggingConfiguration)).Get<LoggingConfiguration>() ??
                         new LoggingConfiguration();
            setup?.Invoke(config);

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

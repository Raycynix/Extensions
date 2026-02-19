using Elastic.CommonSchema.Serilog;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Raycynix.Extensions.Logging.Configurations
{
    /// <summary>
    /// Provides extension methods for integrating Serilog logging with Raycynix microservices.
    /// </summary>
    public static class LoggerConfigurationExtensions
    {

        /// <summary>
        /// Adds and configures the Raycynix logging system with Serilog and Elasticsearch.
        /// </summary>
        /// <param name="services">The current <see cref="IServiceCollection"/>.</param>
        /// <param name="configuration">Application configuration (appsettings.json).</param>
        /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
        public static IServiceCollection AddRaycynixLogging(this IServiceCollection services, IConfiguration configuration)
        {
            var loggingConfiguration = configuration
                .GetSection(nameof(LoggingConfiguration))
                .Get<LoggingConfiguration>() ?? new LoggingConfiguration();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(loggingConfiguration.MinimumLevel)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Service", loggingConfiguration.ServiceName)
                .Enrich.WithProperty("Version", loggingConfiguration.ServiceVersion)
                .WriteTo.Console(theme: SystemConsoleTheme.Colored)
                .WriteTo.Elasticsearch([new Uri(loggingConfiguration.ElasticUrl)], options =>
                {
                    options.MinimumLevel = options.MinimumLevel;
                    options.DataStream = new DataStreamName
                    (
                        "logs",
                        loggingConfiguration.ServiceName.ToLowerInvariant(),
                        loggingConfiguration.Environment.ToLowerInvariant()
                    );
                    options.TextFormatting = new EcsTextFormatterConfiguration<LogEventEcsDocument>();
                })
                .CreateLogger();

            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(Log.Logger, dispose: true);
            });

            return services;
        }
    }
}

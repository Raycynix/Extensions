using Elastic.CommonSchema.Serilog;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Common.Context;
using Raycynix.Extensions.Common.Helpers;
using Raycynix.Extensions.Logging.Configurations;
using Raycynix.Extensions.Logging.Http;
using Raycynix.Extensions.Logging.Internal;
using Serilog;

namespace Raycynix.Extensions.Logging;

/// <summary>
/// Provides extension methods for integrating Serilog logging into an application.
/// </summary>
public static class Logging
{
    /// <summary>
    /// Configures the application to use Raycynix logging with Serilog.
    /// </summary>
    /// <param name="hostBuilder">
    /// The <see cref="IHostBuilder"/> instance used to configure the application.
    /// </param>
    /// <param name="setup">
    /// An optional action to configure additional Serilog settings.
    /// </param>
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
                .Enrich.WithProperty("ServiceName", AssemblyHelper.CurrentName())
                .Enrich.WithProperty("ServiceVersion", AssemblyHelper.CurrentVersion())
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

            //Auto adding raycynix logging and observation to the service collection
            hostBuilder.ConfigureServices((_, serviceCollection) =>
                {
                    serviceCollection.AddRaycynixHttpClients();
                    serviceCollection.AddRaycynixObservation();
                    serviceCollection.AddRaycynixLogging();
                }
            );
        });
    }

    /// <param name="services"></param>
    extension(IServiceCollection services)
    {
        private void AddRaycynixLogging() =>
            services.TryAddSingleton(typeof(Abstractions.ILogger<>), typeof(Implementation.Logger<>));

        private void AddRaycynixObservation() => services.TryAddSingleton<IOperationContext, OperationContext>();

        private void AddRaycynixHttpClients() => services.AddTransient<CorrelationHeaderHandler>();

        private void AddRaycynixMetrics()
        {
            services.AddHealthChecks();
            
            
        }
    }

    //TODO: Create Documentation
}
using Elastic.CommonSchema.Serilog;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Logging.Abstractions;
using Raycynix.Extensions.Logging.Abstractions.Configurations;
using Raycynix.Extensions.Logging.Elastic.Configurations;
using Serilog;

namespace Raycynix.Extensions.Logging.Elastic.Configurators;

/// <summary>
/// Configures the Serilog Elasticsearch sink from Raycynix logging settings.
/// </summary>
public class ElasticLoggingConfigurator : IRaycynixLoggingConfigurator
{
    /// <inheritdoc />
    public void Configure(
        HostBuilderContext context,
        IServiceProvider services,
        LoggerConfiguration loggerConfiguration,
        LoggingConfiguration configuration)
    {
        var elasticConfiguration = services.GetRequiredService<IConfigurationAccessor<ElasticConfiguration>>().Current;

        if (elasticConfiguration is null)
            throw new InvalidOperationException("Elastic configuration is not set");


        if (!elasticConfiguration.Enabled)
            return;

        loggerConfiguration.WriteTo.Elasticsearch([new Uri(elasticConfiguration.Url)], options =>
        {
            options.DataStream = new DataStreamName(
                "logs",
                configuration.ServiceName.ToLowerInvariant(),
                configuration.Environment.ToLowerInvariant()
            );
            options.TextFormatting = new EcsTextFormatterConfiguration<LogEventEcsDocument>();
        });
    }
}

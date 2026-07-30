using Elastic.CommonSchema.Serilog;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Logging.Abstractions;
using Raycynix.Extensions.Logging.Abstractions.Options;
using Raycynix.Extensions.Logging.Elastic.Options;
using Serilog;

namespace Raycynix.Extensions.Logging.Elastic.Configurators;

/// <summary>
/// Configures the Serilog Elasticsearch sink from Raycynix logging settings.
/// </summary>
public class ElasticLoggingConfigurator(
    IConfiguration configuration,
    Action<ElasticOptions>? configure = null) : IRaycynixLoggingConfigurator
{
    /// <inheritdoc />
    public void Configure(
        HostBuilderContext context,
        IServiceProvider services,
        LoggerConfiguration loggerConfiguration,
        LoggingOptions options)
    {
        var elasticOptions = configuration
                                 .GetSection(ConfigurationSectionPath.Combine<LoggingOptions, ElasticOptions>())
                                 .Get<ElasticOptions>()
                             ?? new ElasticOptions();

        configure?.Invoke(elasticOptions);
        elasticOptions.Validate();

        if (!elasticOptions.Enabled)
            return;

        loggerConfiguration.WriteTo.Elasticsearch([new Uri(elasticOptions.Url)], sinkOptions =>
        {
            sinkOptions.DataStream = new DataStreamName(
                "logs",
                options.ServiceName.ToLowerInvariant(),
                options.Environment.ToLowerInvariant()
            );
            sinkOptions.TextFormatting = new EcsTextFormatterConfiguration<LogEventEcsDocument>();
        });
    }
}

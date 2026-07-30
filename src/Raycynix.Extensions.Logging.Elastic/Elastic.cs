using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Logging.Abstractions;
using Raycynix.Extensions.Logging.Abstractions.Options;
using Raycynix.Extensions.Logging.Elastic.Configurators;
using Raycynix.Extensions.Logging.Elastic.Internal;
using Raycynix.Extensions.Logging.Elastic.Options;

namespace Raycynix.Extensions.Logging.Elastic;

/// <summary>
/// Provides Elasticsearch logging integration registration extensions.
/// </summary>
public static class Elastic
{
    /// <summary>
    /// Adds the Elasticsearch Serilog sink to the Raycynix logging pipeline.
    /// </summary>
    /// <param name="builder">The Raycynix logging builder to extend.</param>
    /// <param name="configure">An optional callback for adjusting the bound Elasticsearch logging configuration.</param>
    /// <returns>The same <see cref="LoggingBuilder"/> instance for chaining.</returns>
    public static LoggingBuilder AddElastic(this LoggingBuilder builder, Action<ElasticOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        if (builder.Configuration is null)
            throw new InvalidOperationException(
                "Elastic logging requires AddRaycynixLogging(configuration) or AddElastic(configuration).");

        return builder.AddElastic(builder.Configuration, configure);
    }

    /// <summary>
    /// Adds the Elasticsearch Serilog sink to the Raycynix logging pipeline using the specified configuration source.
    /// </summary>
    /// <param name="builder">The Raycynix logging builder to extend.</param>
    /// <param name="configuration">The application configuration source.</param>
    /// <param name="configure">An optional callback for adjusting the bound Elasticsearch logging configuration.</param>
    /// <returns>The same <see cref="LoggingBuilder"/> instance for chaining.</returns>
    public static LoggingBuilder AddElastic(this LoggingBuilder builder, IConfiguration configuration,
        Action<ElasticOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configuration);

        builder.Services.AddRaycynixConfiguration<ElasticOptions>(
            configuration,
            ConfigurationSectionPath.Combine<LoggingOptions, ElasticOptions>(),
            configurePostBind: configure);

        builder.Services.AddRaycynixConfigurationValidator<ElasticOptions, ElasticOptionsValidator>();

        builder.Services.AddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IConfigurationAccessor<ElasticOptions>>().Current);

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IRaycynixLoggingConfigurator>(
                new ElasticLoggingConfigurator(configuration, configure)));

        return builder;
    }
}

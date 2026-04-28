using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Logging.Abstractions;
using Raycynix.Extensions.Logging.Abstractions.Configurations;
using Raycynix.Extensions.Logging.Elastic.Configurations;
using Raycynix.Extensions.Logging.Elastic.Configurators;

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
    public static LoggingBuilder AddElastic(this LoggingBuilder builder, Action<ElasticConfiguration>? configure = null)
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
        Action<ElasticConfiguration>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configuration);

        builder.Services.AddRaycynixConfiguration<ElasticConfiguration>(
            configuration,
            $"{nameof(LoggingConfiguration)}:{nameof(ElasticConfiguration)}",
            configurePostBind: configure);

        builder.Services.AddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IConfigurationAccessor<ElasticConfiguration>>().Current);

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IRaycynixLoggingConfigurator, ElasticLoggingConfigurator>());

        return builder;
    }
}

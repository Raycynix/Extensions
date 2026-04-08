using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Kafka.Configurations;
using Raycynix.Extensions.Messaging.Kafka.Interfaces;
using Raycynix.Extensions.Messaging.Kafka.Internal;

namespace Raycynix.Extensions.Messaging.Kafka;

/// <summary>
/// Registers Kafka-specific services for Raycynix messaging.
/// </summary>
public static class KafkaMessagingBuilderExtensions
{
    /// <summary>
    /// Enables Kafka publishing for the current messaging builder.
    /// </summary>
    /// <param name="builder">The messaging builder.</param>
    /// <param name="configuration">The application configuration source.</param>
    /// <param name="sectionName">An optional configuration section name. Defaults to <c>KafkaMessagingConfiguration</c>.</param>
    /// <param name="setup">An optional callback for adjusting the bound configuration.</param>
    /// <returns>The same messaging builder instance.</returns>
    public static MessagingBuilder AddKafka(
        this MessagingBuilder builder,
        IConfiguration configuration,
        string? sectionName = null,
        Action<KafkaMessagingConfiguration>? setup = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configuration);

        var options = new KafkaMessagingConfiguration();
        configuration.GetSection(sectionName ?? nameof(KafkaMessagingConfiguration)).Bind(options);
        setup?.Invoke(options);

        return builder.AddKafka(options);
    }

    /// <summary>
    /// Enables Kafka publishing for the current messaging builder.
    /// </summary>
    /// <param name="builder">The messaging builder.</param>
    /// <param name="setup">The Kafka configuration callback.</param>
    /// <returns>The same messaging builder instance.</returns>
    public static MessagingBuilder AddKafka(
        this MessagingBuilder builder,
        Action<KafkaMessagingConfiguration> setup)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(setup);

        var configuration = new KafkaMessagingConfiguration();
        setup(configuration);
        return builder.AddKafka(configuration);
    }

    private static MessagingBuilder AddKafka(
        this MessagingBuilder builder,
        KafkaMessagingConfiguration configuration)
    {
        configuration.Validate();

        builder.Services.TryAddSingleton(configuration);
        builder.Services.TryAddSingleton<IKafkaProducer, KafkaProducer>();
        builder.Services.TryAddSingleton<IKafkaConsumer, KafkaConsumer>();
        builder.Services.AddHostedService<KafkaInboundConsumer>();
        builder.Services.Replace(ServiceDescriptor.Singleton<ITransportMessagePublisher, KafkaMessagePublisher>());
        return builder;
    }
}

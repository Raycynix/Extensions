using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.RabbitMQ.Configurations;
using Raycynix.Extensions.Messaging.RabbitMQ.Interfaces;
using Raycynix.Extensions.Messaging.RabbitMQ.Internal;

namespace Raycynix.Extensions.Messaging.RabbitMQ;

/// <summary>
/// Registers RabbitMQ-specific services for Raycynix messaging.
/// </summary>
public static class RabbitMqMessagingBuilderExtensions
{
    /// <summary>
    /// Enables RabbitMQ publishing for the current messaging builder.
    /// </summary>
    /// <param name="builder">The messaging builder.</param>
    /// <param name="setup">The RabbitMQ configuration callback.</param>
    /// <returns>The same messaging builder instance.</returns>
    public static MessagingBuilder AddRabbitMq(
        this MessagingBuilder builder,
        Action<RabbitMqMessagingConfiguration> setup)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(setup);

        var configuration = new RabbitMqMessagingConfiguration();
        setup(configuration);
        configuration.Validate();

        builder.Services.TryAddSingleton(configuration);
        builder.Services.TryAddSingleton<IRabbitMqConnectionFactory, RabbitMqClientConnectionFactory>();
        builder.Services.TryAddSingleton<RabbitMqConnectionAccessor>();
        builder.Services.AddHostedService<RabbitMqInboundConsumer>();
        builder.Services.Replace(ServiceDescriptor.Singleton<IMessagePublisher, RabbitMqMessagePublisher>());
        return builder;
    }
}

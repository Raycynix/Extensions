using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Abstractions.Interfaces;
using Raycynix.Extensions.Messaging.Configurations;
using Raycynix.Extensions.Messaging.Implementations;
using Raycynix.Extensions.Messaging.Internal;
using Raycynix.Extensions.Messaging.Serialization;

namespace Raycynix.Extensions.Messaging;

/// <summary>
/// Provides service registration extensions for transport-agnostic Raycynix messaging.
/// </summary>
public static class Messaging
{
    /// <summary>
    /// Registers transport-agnostic messaging services, codecs, and envelope helpers.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="configuration">The application configuration source.</param>
    /// <param name="setup">An optional callback for adjusting the bound messaging configuration.</param>
    /// <returns>A messaging builder for provider-specific extensions.</returns>
    public static MessagingBuilder AddRaycynixMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<MessagingConfiguration>? setup = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddRaycynixConfiguration<MessagingConfiguration>(configuration, configurePostBind: setup);
        services.AddRaycynixConfigurationValidator<MessagingConfiguration, MessageConfigurationValidator>();
        services.AddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IConfigurationAccessor<MessagingConfiguration>>().Current);

        services.AddSingleton<IMessageContractResolver, MessageContractResolver>();
        services.AddSingleton<MessageHeaderEnricher>();
        services.AddSingleton<IMessageEnvelopeFactory, MessageEnvelopeFactory>();
        services.AddSingleton<IRequestEnvelopeFactory, RequestEnvelopeFactory>();
        services.AddSingleton<IMessageDispatcher, MessageDispatcher>();
        services.AddSingleton<IMessageCodecResolver, MessageCodecResolver>();
        services.AddSingleton<IMessageSerializer, MessageSerializer>();
        services.AddSingleton<MessageObservability>();
        services.AddSingleton<IMessagePublisher, MessagePublisher>();
        services.AddSingleton<IDirectRequestClient, DirectRequestClient>();
        services.AddSingleton<IMessageCodec, NewtonsoftJsonMessageCodec>();

        return new MessagingBuilder(services);
    }
}

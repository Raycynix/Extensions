using Messaging.Abstractions;
using Messaging.Implementation.Core;
using Messaging.Implementation.Kafka;
using Messaging.Implementation.Rabbit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Messaging.Configurations
{
    /// <summary>
    /// Provides DI extensions for configuring Raycynix Messaging (RabbitMQ / Kafka)
    /// with automatic handler registration and discovery.
    /// </summary>
    public static class MessagingSetupExtensions
    {
        /// <summary>
        /// Adds Raycynix unified messaging system to the service collection.
        /// Automatically registers <see cref="IMessageHandler{T}"/> implementations.
        /// </summary>
        public static IServiceCollection AddRaycynixMessaging(this IServiceCollection services, IConfiguration config)
        {
            var options = config.GetSection(nameof(MessagingConfiguration)).Get<MessagingConfiguration>() ?? new MessagingConfiguration();

            services.AddSingleton(options);
            services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();
            services.AddSingleton<IMessageBus, MessageBus>();
            services.AddSingleton<MessageSubscriptionManager>();

            RegisterMessageHandlers(services);

            if (options.EnableRabbitMq)
            {
                services.AddSingleton<IMessageProducer, RabbitMqProducer>();
                services.AddSingleton<IMessageConsumer, RabbitMqConsumer>();
            }

            if (options.EnableKafka)
            {
                services.AddSingleton<IMessageProducer, KafkaProducer>();
                services.AddSingleton<IMessageConsumer, KafkaConsumer>();
            }

            return services;
        }


        /// <summary>
        /// Scans assemblies for <see cref="IMessageHandler{T}"/> implementations and registers them in DI.
        /// </summary>
        private static void RegisterMessageHandlers(IServiceCollection services)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.FullName));

            foreach (var assembly in assemblies)
            {
                var handlerTypes = assembly.GetTypes()
                    .Where(t => !t.IsAbstract && !t.IsInterface)
                    .SelectMany(t => t.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>))
                        .Select(i => new { Handler = t, Interface = i }));

                foreach (var h in handlerTypes)
                {
                    services.AddTransient(h.Interface, h.Handler);
                }
            }
        }
    }
}

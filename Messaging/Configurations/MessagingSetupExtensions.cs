using Messaging.Abstractions;
using Messaging.Implementation.Core;
using Messaging.Implementation.Kafka;
using Messaging.Implementation.Rabbit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Messaging.Configurations
{
    /// <summary>
    /// Adds Raycynix messaging support (RabbitMQ / Kafka) to the DI container.
    /// </summary>
    public static class MessagingSetupExtensions
    {
        public static IServiceCollection AddRaycynixMessaging(this IServiceCollection services, IConfiguration config)
        {
            var options = config.GetSection("Messaging").Get<MessagingConfiguration>() ?? new MessagingConfiguration();

            services.AddSingleton(options);
            services.AddSingleton<IMessageBus, MessageBus>();

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
    }
}

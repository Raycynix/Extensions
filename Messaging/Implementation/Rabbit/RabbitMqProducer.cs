using System.Text;
using Messaging.Abstractions;
using Messaging.Configurations;
using RabbitMQ.Client;

namespace Messaging.Implementation.Rabbit
{
    /// <summary>
    /// RabbitMQ producer implementation for publishing messages.
    /// </summary>
    public class RabbitMqProducer : IMessageProducer, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly MessagingConfiguration _configuration;
        private readonly IMessageSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMqProducer"/> class.
        /// </summary>
        public RabbitMqProducer(MessagingConfiguration configuration, IMessageSerializer serializer)
        {
            _configuration = configuration;
            _serializer = serializer;

            var factory = new ConnectionFactory
            {
                HostName = configuration.RabbitMq.Host,
                UserName = configuration.RabbitMq.User,
                Password = configuration.RabbitMq.Password
            };

            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
            _channel.ExchangeDeclareAsync(
                exchange: _configuration.RabbitMq.Exchange,
                type: "topic",
                durable: true,
                autoDelete: false,
                arguments: null).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public Task PublishAsync<T>(string topic, T message, CancellationToken cancellationToken = default)
        {
            var json = _serializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var props = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent
            };

            // Используем асинхронный метод публикации без generic-параметра, передавая null для basicProperties
            return _channel.BasicPublishAsync(
                exchange: _configuration.RabbitMq.Exchange,
                routingKey: topic,
                mandatory: false,
                basicProperties: props,
                body: body,
                cancellationToken: cancellationToken
            ).AsTask();
        }

        /// <summary>
        /// Closes channel and connection gracefully.
        /// </summary>
        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

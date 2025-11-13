using System.Text;
using Messaging.Abstractions;
using Messaging.Configurations;
using Messaging.Implementation.Core;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Messaging.Implementation.Rabbit
{
    /// <summary>
    /// Asynchronous RabbitMQ consumer for Raycynix Messaging.
    /// </summary>
    public class RabbitMqConsumer : IMessageConsumer, IDisposable
    {
        private readonly MessagingConfiguration _configuration;
        private readonly IMessageSerializer _serializer;
        private readonly MessageSubscriptionManager _subscriptions;
        private readonly IConnection _connection;
        private readonly IChannel _channel;

        public RabbitMqConsumer(
            MessagingConfiguration configuration,
            IMessageSerializer serializer,
            MessageSubscriptionManager subscriptions)
        {
            _configuration = configuration;
            _serializer = serializer;
            _subscriptions = subscriptions;

            var factory = new ConnectionFactory
            {
                HostName = _configuration.RabbitMq.Host,
                UserName = _configuration.RabbitMq.User,
                Password = _configuration.RabbitMq.Password,
            };

            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public void Subscribe<TMessage, THandler>()
            where THandler : IMessageHandler<TMessage>
        {
            var queue = typeof(TMessage).Name.ToLowerInvariant();
            _channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false);

            _channel.QueueBindAsync(
                queue: queue,
                exchange: _configuration.RabbitMq.Exchange,
                routingKey: queue);

            _subscriptions.Register<TMessage, THandler>();
        }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var typeHeader = ea.BasicProperties.Type ?? string.Empty;

                await _subscriptions.HandleAsync(typeHeader, body, _serializer, cancellationToken);
            };

            await _channel.BasicConsumeAsync(queue: "#", autoAck: true, consumer: consumer, cancellationToken: cancellationToken);
        }

        public void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

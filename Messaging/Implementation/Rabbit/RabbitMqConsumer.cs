using System.Text;
using Messaging.Abstractions;
using Messaging.Configurations;
using Messaging.Implementation.Core;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Messaging.Implementation.Rabbit
{
    /// <summary>
    /// Represents an asynchronous RabbitMQ consumer within the Raycynix Messaging framework.
    /// </summary>
    /// <remarks>
    /// The <see cref="RabbitMqConsumer"/> automatically discovers and subscribes to all
    /// <see cref="IMessageHandler{T}"/> implementations registered in the dependency container.
    /// It binds queues to the configured exchange and dispatches received messages to the
    /// appropriate message handlers via <see cref="MessageSubscriptionManager"/>.
    /// </remarks>
    public class RabbitMqConsumer : IMessageConsumer, IDisposable
    {
        private readonly MessagingConfiguration _configuration;
        private readonly IMessageSerializer _serializer;
        private readonly MessageSubscriptionManager _subscriptions;
        private readonly IConnection _connection;
        private readonly IChannel _channel;

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMqConsumer"/> class.
        /// </summary>
        /// <param name="configuration">The messaging configuration options for RabbitMQ.</param>
        /// <param name="serializer">The message serializer used for converting message payloads.</param>
        /// <param name="subscriptions">The message subscription manager for routing messages to handlers.</param>
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
        /// <summary>
        /// Subscribes a message type and its handler to a specific queue.
        /// This method can be used manually, but in most cases handlers are discovered automatically.
        /// </summary>
        /// <typeparam name="TMessage">The message type.</typeparam>
        /// <typeparam name="THandler">The handler type that processes this message.</typeparam>
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
        /// <summary>
        /// Starts consuming messages from RabbitMQ.
        /// Automatically discovers message handlers and creates queue bindings for them.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for graceful shutdown.</param>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            // Discover all message handlers dynamically
            _subscriptions.AutoDiscoverHandlers();

            // Bind all queues
            foreach (var msgType in _subscriptions.GetAllMessageTypes())
            {
                var queue = msgType.Name.ToLowerInvariant();
                await _channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
                await _channel.QueueBindAsync(queue, _configuration.RabbitMq.Exchange, routingKey: queue, cancellationToken: cancellationToken);
            }

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var typeHeader = ea.BasicProperties.Type ?? string.Empty;

                await _subscriptions.HandleAsync(typeHeader, body, _serializer, cancellationToken);
            };


            // Consume all discovered queues
            foreach (var msgType in _subscriptions.GetAllMessageTypes())
            {
                await _channel.BasicConsumeAsync(queue: "#", autoAck: true, consumer: consumer, cancellationToken: cancellationToken);
            }
        }

        /// <summary>
        /// Releases all resources associated with the RabbitMQ consumer.
        /// </summary>
        /// <remarks>
        /// Closes both the channel and the connection to RabbitMQ broker.
        /// </remarks>
        public void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

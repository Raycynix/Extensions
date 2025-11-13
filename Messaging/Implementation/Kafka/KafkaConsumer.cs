using Confluent.Kafka;
using Messaging.Abstractions;
using Messaging.Configurations;
using Messaging.Implementation.Core;
using Microsoft.Extensions.Logging;

namespace Messaging.Implementation.Kafka
{
    /// <summary>
    /// Represents a Kafka consumer used within the Raycynix Messaging framework.
    /// </summary>
    /// <remarks>
    /// The <see cref="KafkaConsumer"/> automatically discovers all message handlers
    /// implementing <see cref="IMessageHandler{T}"/>, subscribes to their respective
    /// topics (based on message type names), and dispatches incoming messages
    /// to the correct handler via <see cref="MessageSubscriptionManager"/>.
    /// </remarks>
    public class KafkaConsumer : IMessageConsumer, IDisposable
    {
        private readonly MessagingConfiguration _configuration;
        private readonly IMessageSerializer _serializer;
        private readonly MessageSubscriptionManager _subscriptions;
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly ILogger<KafkaConsumer>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaConsumer"/> class.
        /// </summary>
        /// <param name="configuration">The messaging configuration options.</param>
        /// <param name="serializer">The message serializer used for deserialization.</param>
        /// <param name="subscriptions">The subscription manager that maps messages to handlers.</param>
        /// <param name="logger">Optional logger instance for diagnostic information.</param>
        public KafkaConsumer(
            MessagingConfiguration configuration,
            IMessageSerializer serializer,
            MessageSubscriptionManager subscriptions,
            ILogger<KafkaConsumer>? logger = null)
        {
            _configuration = configuration;
            _serializer = serializer;
            _subscriptions = subscriptions;
            _logger = logger;

            var config = new ConsumerConfig
            {
                BootstrapServers = configuration.Kafka.BootstrapServers,
                GroupId = configuration.Kafka.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        }

        /// <inheritdoc/>
        public void Subscribe<TMessage, THandler>()
            where THandler : IMessageHandler<TMessage>
        {
            var topic = typeof(TMessage).Name.ToLowerInvariant();
            _consumer.Subscribe(topic);
            _subscriptions.Register<TMessage, THandler>();
        }

        /// <inheritdoc/>
        /// <summary>
        /// Starts the Kafka consumer asynchronously.
        /// It automatically subscribes to all discovered message types
        /// and dispatches incoming messages to the corresponding handlers.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for graceful shutdown.</param>
        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            _subscriptions.AutoDiscoverHandlers();

            var topics = _subscriptions.GetAllMessageTypes().Select(t => t.Name.ToLowerInvariant()).ToList();
            _consumer.Subscribe(topics);

            return Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = _consumer.Consume(cancellationToken);

                        // Determine the message type from the topic name
                        var typeName = result.Topic switch
                        {
                            { Length: > 0 } => result.Topic,
                            _ => typeof(object).FullName!
                        };

                        await _subscriptions.HandleAsync(typeName, result.Message.Value, _serializer, cancellationToken);
                    }
                    catch (ConsumeException ex)
                    {
                        _logger?.LogError(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger?.LogInformation("Kafka consumer stopped by cancellation.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Kafka consumer error");
                    }
                }
            }, cancellationToken);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _consumer.Close();
            GC.SuppressFinalize(this);
        }
    }
}

using Confluent.Kafka;
using Messaging.Abstractions;
using Messaging.Configurations;
using Messaging.Implementation.Core;
using Microsoft.Extensions.Logging;

namespace Messaging.Implementation.Kafka
{
    /// <summary>
    /// Kafka consumer that routes incoming messages to registered handlers.
    /// </summary>
    public class KafkaConsumer : IMessageConsumer, IDisposable
    {
        private readonly MessagingConfiguration _configuration;
        private readonly IMessageSerializer _serializer;
        private readonly MessageSubscriptionManager _subscriptions;
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly ILogger<KafkaConsumer>? _logger;

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
                        var typeName = result.Message.Headers
                            .FirstOrDefault(h => h.Key == "TypeName")?
                            .GetValueBytes() is { } bytes
                            ? System.Text.Encoding.UTF8.GetString(bytes)
                            : typeof(object).FullName!;

                        await _subscriptions.HandleAsync(typeName, result.Message.Value, _serializer, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Kafka consumer error");
                    }
                }
            }, cancellationToken);
        }

        public void Dispose()
        {
            _consumer.Close();
            GC.SuppressFinalize(this);
        }
    }
}

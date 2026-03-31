using Raycynix.Extensions.Messaging.RabbitMQ.Configurations;

namespace Raycynix.Extensions.Messaging.RabbitMQ.Internal;

/// <summary>
/// Manages RabbitMQ connection and topology initialization.
/// </summary>
internal sealed class RabbitMqConnectionAccessor : IDisposable, IAsyncDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly RabbitMqMessagingConfiguration _configuration;
    private readonly IRabbitMqConnectionFactory _connectionFactory;
    private IRabbitMqConnection? _connection;
    private bool _topologyInitialized;

    /// <summary>
    /// Initializes a new accessor instance.
    /// </summary>
    public RabbitMqConnectionAccessor(
        RabbitMqMessagingConfiguration configuration,
        IRabbitMqConnectionFactory connectionFactory)
    {
        this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this._connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    /// <summary>
    /// Creates a configured channel and ensures topology exists.
    /// </summary>
    public async Task<IRabbitMqChannel> CreateChannelAsync(CancellationToken cancellationToken)
    {
        var activeConnection = await EnsureConnectionAsync(cancellationToken).ConfigureAwait(false);
        await EnsureTopologyAsync(activeConnection, cancellationToken).ConfigureAwait(false);

        var channel = await activeConnection.CreateChannelAsync(cancellationToken).ConfigureAwait(false);
        await channel.BasicQosAsync(0, _configuration.Queue.PrefetchCount, false, cancellationToken).ConfigureAwait(false);
        return channel;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
        }

        _semaphore.Dispose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    private async Task<IRabbitMqConnection> EnsureConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is not null)
        {
            return _connection;
        }

        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _connection ??= await _connectionFactory.CreateAsync(_configuration, cancellationToken).ConfigureAwait(false);
            return _connection;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task EnsureTopologyAsync(IRabbitMqConnection activeConnection, CancellationToken cancellationToken)
    {
        if (_topologyInitialized)
        {
            return;
        }

        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_topologyInitialized)
            {
                return;
            }

            await using var channel = await activeConnection.CreateChannelAsync(cancellationToken).ConfigureAwait(false);
            var queueArguments = BuildQueueArguments();

            await channel.ExchangeDeclareAsync(
                    _configuration.Exchange.Name,
                    _configuration.Exchange.Type,
                    _configuration.Exchange.Durable,
                    _configuration.Exchange.AutoDelete,
                    arguments: null,
                    passive: false,
                    noWait: false,
                    cancellationToken)
                .ConfigureAwait(false);

            await channel.QueueDeclareAsync(
                    _configuration.Queue.Name,
                    _configuration.Queue.Durable,
                    _configuration.Queue.Exclusive,
                    _configuration.Queue.AutoDelete,
                    queueArguments,
                    passive: false,
                    noWait: false,
                    cancellationToken)
                .ConfigureAwait(false);

            await channel.QueueBindAsync(
                    _configuration.Queue.Name,
                    _configuration.Exchange.Name,
                    _configuration.Queue.Name,
                    arguments: null,
                    noWait: false,
                    cancellationToken)
                .ConfigureAwait(false);

            if (_configuration.DeadLetter.Enabled)
            {
                await channel.ExchangeDeclareAsync(
                        _configuration.DeadLetter.Exchange,
                        "topic",
                        durable: true,
                        autoDelete: false,
                        arguments: null,
                        passive: false,
                        noWait: false,
                        cancellationToken)
                    .ConfigureAwait(false);

                await channel.QueueDeclareAsync(
                        _configuration.DeadLetter.Queue,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null,
                        passive: false,
                        noWait: false,
                        cancellationToken)
                    .ConfigureAwait(false);

                await channel.QueueBindAsync(
                        _configuration.DeadLetter.Queue,
                        _configuration.DeadLetter.Exchange,
                        _configuration.DeadLetter.RoutingKey,
                        arguments: null,
                        noWait: false,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            _topologyInitialized = true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private Dictionary<string, object?> BuildQueueArguments()
    {
        var queueArguments = new Dictionary<string, object?>();

        if (_configuration.Queue.MessageTtlMilliseconds is { } ttl)
        {
            queueArguments["x-message-ttl"] = ttl;
        }

        if (_configuration.DeadLetter.Enabled)
        {
            queueArguments["x-dead-letter-exchange"] = _configuration.DeadLetter.Exchange;
            queueArguments["x-dead-letter-routing-key"] = _configuration.DeadLetter.RoutingKey;
        }

        return queueArguments;
    }
}

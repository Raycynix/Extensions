namespace Raycynix.Extensions.Messaging.RabbitMQ.Configurations;

/// <summary>
/// Configures RabbitMQ connectivity and topology.
/// </summary>
public sealed class RabbitMqMessagingConfiguration
{
    /// <summary>
    /// Gets the RabbitMQ host name.
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Gets the RabbitMQ port.
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Gets the RabbitMQ virtual host.
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Gets the RabbitMQ username.
    /// </summary>
    public string Username { get; set; } = "guest";

    /// <summary>
    /// Gets the RabbitMQ password.
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Gets a value indicating whether TLS is enabled.
    /// </summary>
    public bool UseSsl { get; set; }

    /// <summary>
    /// Gets the inbound consumer configuration.
    /// </summary>
    public RabbitMqConsumerConfiguration Consumer { get; set; } = new();

    /// <summary>
    /// Gets the exchange configuration.
    /// </summary>
    public RabbitMqExchangeConfiguration Exchange { get; set; } = new();

    /// <summary>
    /// Gets the queue configuration.
    /// </summary>
    public RabbitMqQueueConfiguration Queue { get; set; } = new();

    /// <summary>
    /// Gets the retry configuration.
    /// </summary>
    public RabbitMqRetryConfiguration Retry { get; set; } = new();

    /// <summary>
    /// Gets the dead-letter configuration.
    /// </summary>
    public RabbitMqDeadLetterConfiguration DeadLetter { get; set; } = new();

    /// <summary>
    /// Validates the RabbitMQ configuration.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Host))
        {
            throw new InvalidOperationException("RabbitMQ host cannot be empty.");
        }

        if (Port <= 0)
        {
            throw new InvalidOperationException("RabbitMQ port must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(Exchange.Name))
        {
            throw new InvalidOperationException("RabbitMQ exchange name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(Queue.Name))
        {
            throw new InvalidOperationException("RabbitMQ queue name cannot be empty.");
        }

        if (Retry.MaxAttempts < 1)
        {
            throw new InvalidOperationException("RabbitMQ retry max attempts must be at least 1.");
        }

        if (Retry.DelayMilliseconds < 0)
        {
            throw new InvalidOperationException("RabbitMQ retry delay cannot be negative.");
        }

        if (Consumer.PollIntervalMilliseconds < 0)
        {
            throw new InvalidOperationException("RabbitMQ consumer poll interval cannot be negative.");
        }
    }
}

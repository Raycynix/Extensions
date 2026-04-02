namespace Raycynix.Extensions.Messaging.RabbitMQ.Configurations;

/// <summary>
/// Configures RabbitMQ retry behavior.
/// </summary>
public sealed class RabbitMqRetryConfiguration
{
    /// <summary>
    /// Gets a value indicating whether retries are enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets the maximum retry attempts.
    /// </summary>
    public int MaxAttempts { get; set; } = 5;

    /// <summary>
    /// Gets the retry delay in milliseconds.
    /// </summary>
    public int DelayMilliseconds { get; set; } = 5000;
}

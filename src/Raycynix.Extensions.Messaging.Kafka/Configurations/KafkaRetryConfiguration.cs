namespace Raycynix.Extensions.Messaging.Kafka.Configurations;

/// <summary>
/// Configures Kafka retry behavior for failed inbound message processing.
/// </summary>
public sealed class KafkaRetryConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether failed inbound messages should be republished for retry.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of processing attempts before a message is dead-lettered or dropped.
    /// </summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the delay before republishing a failed message for retry.
    /// </summary>
    public int DelayMilliseconds { get; set; }

    /// <summary>
    /// Validates the retry configuration.
    /// </summary>
    public void Validate()
    {
        if (MaxAttempts <= 0)
        {
            throw new InvalidOperationException("Kafka retry max attempts must be greater than zero.");
        }

        if (DelayMilliseconds < 0)
        {
            throw new InvalidOperationException("Kafka retry delay cannot be negative.");
        }
    }
}

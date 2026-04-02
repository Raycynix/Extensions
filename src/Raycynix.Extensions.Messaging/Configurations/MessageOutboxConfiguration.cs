namespace Raycynix.Extensions.Messaging.Configurations;

/// <summary>
/// Configures outgoing outbox storage and publish recovery behavior.
/// </summary>
public sealed class MessageOutboxConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether outgoing messages should be stored in the outbox before publishing.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether publishing should be attempted immediately after enqueuing.
    /// </summary>
    public bool AutoDispatchOnPublish { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether background recovery should republish pending and failed messages.
    /// </summary>
    public bool EnableRecovery { get; set; } = true;

    /// <summary>
    /// Gets or sets the delay between recovery polls.
    /// </summary>
    public TimeSpan RecoveryInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets the maximum number of messages processed in a single recovery cycle.
    /// </summary>
    public int RecoveryBatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the delay before a failed message becomes eligible for retry.
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the lease timeout for a message that is currently being dispatched.
    /// </summary>
    public TimeSpan DispatchLeaseTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Validates the outbox configuration.
    /// </summary>
    public void Validate()
    {
        if (RecoveryInterval <= TimeSpan.Zero)
        {
            throw new InvalidOperationException("Outbox recovery interval must be greater than zero.");
        }

        if (RecoveryBatchSize <= 0)
        {
            throw new InvalidOperationException("Outbox recovery batch size must be greater than zero.");
        }

        if (RetryDelay < TimeSpan.Zero)
        {
            throw new InvalidOperationException("Outbox retry delay cannot be negative.");
        }

        if (DispatchLeaseTimeout <= TimeSpan.Zero)
        {
            throw new InvalidOperationException("Outbox dispatch lease timeout must be greater than zero.");
        }
    }
}

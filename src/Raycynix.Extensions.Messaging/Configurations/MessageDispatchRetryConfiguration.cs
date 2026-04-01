namespace Raycynix.Extensions.Messaging.Configurations;

/// <summary>
/// Configures retry behavior for incoming message dispatch operations.
/// </summary>
public sealed class MessageDispatchRetryConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether dispatch retries are enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of retries after the initial failed attempt.
    /// </summary>
    public int MaxRetries { get; set; }

    /// <summary>
    /// Gets or sets the base delay between retry attempts.
    /// </summary>
    public TimeSpan Delay { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Gets or sets a value indicating whether retry delays use exponential backoff.
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;

    /// <summary>
    /// Validates the current retry configuration.
    /// </summary>
    public void Validate()
    {
        if (MaxRetries < 0)
        {
            throw new InvalidOperationException("Dispatch retry max retries cannot be negative.");
        }

        if (Delay < TimeSpan.Zero)
        {
            throw new InvalidOperationException("Dispatch retry delay cannot be negative.");
        }
    }
}

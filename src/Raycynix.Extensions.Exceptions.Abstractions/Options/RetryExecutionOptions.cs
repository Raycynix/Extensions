namespace Raycynix.Extensions.Exceptions.Abstractions.Options;

/// <summary>
/// Represents options controlling retry execution behavior.
/// </summary>
public class RetryExecutionOptions
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// </summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>
    /// Gets or sets the base delay between retries.
    /// </summary>
    public TimeSpan Delay { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets a value indicating whether exponential backoff is enabled.
    /// </summary>
    public bool UseExponentialBackoff { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether a small random jitter is added to each delay.
    /// </summary>
    public bool UseJitter { get; init; } = true;

    /// <summary>
    /// Validates the current retry options.
    /// </summary>
    public void Validate()
    {
        if (MaxRetries < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxRetries), "Max retries cannot be negative.");
        }

        if (Delay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(Delay), "Retry delay cannot be negative.");
        }
    }
}
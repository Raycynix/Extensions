namespace Raycynix.Extensions.Exceptions.Abstractions.Options;

/// <summary>
/// Represents options controlling retry execution behavior.
/// </summary>
public class RetryExecutionOptions
{
    private const int MaximumRetryCount = 1000;
    private static readonly TimeSpan MaximumSupportedDelay = TimeSpan.FromMilliseconds(uint.MaxValue - 1D);

    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// </summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>
    /// Gets or sets the base delay between retries.
    /// </summary>
    public TimeSpan Delay { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets the maximum delay allowed between retry attempts.
    /// </summary>
    public TimeSpan MaxDelay { get; init; } = TimeSpan.FromMinutes(1);

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

        if (MaxRetries > MaximumRetryCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaxRetries),
                $"Max retries cannot exceed {MaximumRetryCount}.");
        }

        if (Delay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(Delay), "Retry delay cannot be negative.");
        }


        if (MaxDelay <= TimeSpan.Zero || MaxDelay > MaximumSupportedDelay)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaxDelay),
                $"Maximum retry delay must be positive and cannot exceed {MaximumSupportedDelay}.");
        }
    }
}

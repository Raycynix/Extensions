namespace Raycynix.Extensions.Security.AspNetCore.Enums;

/// <summary>
/// Identifies the algorithm used by a Raycynix rate limit policy.
/// </summary>
public enum RateLimitAlgorithm
{
    /// <summary>
    /// Limits requests within discrete time windows.
    /// </summary>
    FixedWindow,

    /// <summary>
    /// Limits requests across a window divided into rolling segments.
    /// </summary>
    SlidingWindow,

    /// <summary>
    /// Replenishes request permits at a configured interval.
    /// </summary>
    TokenBucket,

    /// <summary>
    /// Limits the number of concurrent requests.
    /// </summary>
    Concurrency
}

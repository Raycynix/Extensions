namespace Raycynix.Extensions.Security.AspNetCore.Enums;

/// <summary>
/// Identifies how requests are partitioned into independent rate limit buckets.
/// </summary>
public enum RateLimitPartitionStrategy
{
    /// <summary>
    /// Creates a bucket for each remote IP address.
    /// </summary>
    IpAddress,

    /// <summary>
    /// Creates a bucket for each authenticated JWT subject and falls back to the remote IP address.
    /// </summary>
    Subject,

    /// <summary>
    /// Places every request in one shared bucket.
    /// </summary>
    Global
}

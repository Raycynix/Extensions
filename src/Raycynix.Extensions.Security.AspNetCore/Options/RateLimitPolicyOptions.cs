using System.Threading.RateLimiting;
using Raycynix.Extensions.Security.AspNetCore.Enums;

namespace Raycynix.Extensions.Security.AspNetCore.Options;

/// <summary>
/// Represents one global or named rate limit policy.
/// </summary>
public sealed class RateLimitPolicyOptions
{
    /// <summary>
    /// Gets or sets the rate limiting algorithm.
    /// </summary>
    public RateLimitAlgorithm Algorithm { get; set; } = RateLimitAlgorithm.FixedWindow;

    /// <summary>
    /// Gets or sets how requests are partitioned into independent buckets.
    /// </summary>
    public RateLimitPartitionStrategy PartitionStrategy { get; set; } = RateLimitPartitionStrategy.IpAddress;

    /// <summary>
    /// Gets or sets the permit, token, or concurrency limit.
    /// </summary>
    public int PermitLimit { get; set; } = 100;

    /// <summary>
    /// Gets or sets the fixed or sliding window, or the token replenishment period.
    /// </summary>
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets the number of segments used by the sliding-window algorithm.
    /// </summary>
    public int SegmentsPerWindow { get; set; } = 6;

    /// <summary>
    /// Gets or sets the number of tokens added during each token-bucket replenishment period.
    /// </summary>
    public int TokensPerPeriod { get; set; } = 10;

    /// <summary>
    /// Gets or sets the maximum number of requests waiting for permits.
    /// </summary>
    public int QueueLimit { get; set; }

    /// <summary>
    /// Gets or sets the order in which queued requests are processed.
    /// </summary>
    public QueueProcessingOrder QueueProcessingOrder { get; set; } = QueueProcessingOrder.OldestFirst;

    /// <summary>
    /// Gets or sets whether window and token limiters replenish automatically.
    /// </summary>
    public bool AutoReplenishment { get; set; } = true;
}

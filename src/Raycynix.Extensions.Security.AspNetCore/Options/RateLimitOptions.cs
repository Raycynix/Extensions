using Microsoft.AspNetCore.Http;

namespace Raycynix.Extensions.Security.AspNetCore.Options;

/// <summary>
/// Represents the configurable Raycynix ASP.NET Core rate limiting settings.
/// </summary>
public sealed class RateLimitOptions
{
    /// <summary>
    /// Gets or sets the policy applied to every request. Set to <see langword="null"/> to disable the global limiter.
    /// </summary>
    public RateLimitPolicyOptions? GlobalPolicy { get; set; } = new();

    /// <summary>
    /// Gets or sets named policies that can be selected with ASP.NET Core endpoint metadata.
    /// </summary>
    public Dictionary<string, RateLimitPolicyOptions> Policies { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the HTTP status code returned when a request is rejected.
    /// </summary>
    public int RejectionStatusCode { get; set; } = StatusCodes.Status429TooManyRequests;

    /// <summary>
    /// Gets or sets whether the response includes a Retry-After header when the limiter provides one.
    /// </summary>
    public bool IncludeRetryAfterHeader { get; set; } = true;
}

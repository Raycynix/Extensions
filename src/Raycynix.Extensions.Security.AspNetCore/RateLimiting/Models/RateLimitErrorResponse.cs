namespace Raycynix.Extensions.Security.AspNetCore.RateLimiting.Models;

/// <summary>
/// Represents the response returned when a request exceeds its rate limit.
/// </summary>
public sealed record RateLimitErrorResponse(
    int Status,
    string Code,
    string Message,
    string? TraceId);

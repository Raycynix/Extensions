namespace Raycynix.Extensions.Security.AspNetCore.Authorization.Models;

/// <summary>
/// Represents a safe and predictable error response for authentication and authorization failures.
/// </summary>
public sealed record AuthorizationErrorResponse(
    int Status,
    string Code,
    string Message,
    string? TraceId);

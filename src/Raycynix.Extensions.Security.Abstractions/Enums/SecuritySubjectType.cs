namespace Raycynix.Extensions.Security.Abstractions.Enums;

/// <summary>
/// Represents the type of authenticated subject carried by the security context.
/// </summary>
public enum SecuritySubjectType
{
    /// <summary>
    /// The request is performed on behalf of an authenticated user.
    /// </summary>
    User = 0,

    /// <summary>
    /// The request is performed on behalf of an authenticated service.
    /// </summary>
    Service = 1,
}

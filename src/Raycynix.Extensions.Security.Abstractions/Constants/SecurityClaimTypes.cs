namespace Raycynix.Extensions.Security.Abstractions.Constants;

/// <summary>
/// Provides the shared claim names used by Raycynix security tokens.
/// </summary>
public static class SecurityClaimTypes
{
    /// <summary>
    /// Identifies whether the authenticated subject is a user or a service.
    /// </summary>
    public const string SubjectType = "subject_type";

    /// <summary>
    /// Contains the roles assigned to the authenticated subject.
    /// </summary>
    public const string Roles = "roles";

    /// <summary>
    /// Contains the permissions granted to the authenticated subject.
    /// </summary>
    public const string Permissions = "permissions";
}

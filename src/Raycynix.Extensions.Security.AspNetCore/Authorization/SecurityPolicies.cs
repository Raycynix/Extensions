using Raycynix.Extensions.Security.Abstractions.Enums;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization;

/// <summary>
/// Provides shared policy names and naming conventions for Raycynix ASP.NET Core authorization.
/// </summary>
public static class SecurityPolicies
{
    /// <summary>
    /// A policy that allows access only to authenticated user subjects.
    /// </summary>
    public const string UserOnly = "subject:user";

    /// <summary>
    /// A policy that allows access only to authenticated service subjects.
    /// </summary>
    public const string ServiceOnly = "subject:service";

    internal const string PermissionPrefix = "permission:";
    internal const string RolePrefix = "role:";
    internal const string SubjectPrefix = "subject:";

    /// <summary>
    /// Builds a permission-based policy name.
    /// </summary>
    /// <param name="permission">The required permission.</param>
    /// <returns>A policy name understood by the Raycynix policy provider.</returns>
    public static string Permission(string permission)
    {
        return $"{PermissionPrefix}{permission}";
    }

    /// <summary>
    /// Builds a role-based policy name.
    /// </summary>
    /// <param name="role">The required role.</param>
    /// <returns>A policy name understood by the Raycynix policy provider.</returns>
    public static string Role(string role)
    {
        return $"{RolePrefix}{role}";
    }

    /// <summary>
    /// Builds a subject-type-based policy name.
    /// </summary>
    /// <param name="subjectType">The required authenticated subject type.</param>
    /// <returns>A policy name understood by the Raycynix policy provider.</returns>
    public static string Subject(SecuritySubjectType subjectType)
    {
        return $"{SubjectPrefix}{subjectType.ToString().ToLowerInvariant()}";
    }
}

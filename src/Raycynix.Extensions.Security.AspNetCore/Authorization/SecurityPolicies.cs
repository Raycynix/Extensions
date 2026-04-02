using Raycynix.Extensions.Security.Abstractions.Enums;
using Raycynix.Extensions.Security.Abstractions.Attributes;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization;

/// <summary>
/// Provides shared policy names and naming conventions for Raycynix ASP.NET Core authorization.
/// </summary>
public static class SecurityPolicies
{
    /// <summary>
    /// A policy that allows access to any authenticated subject.
    /// </summary>
    public const string Authenticated = "authenticated";

    /// <summary>
    /// A policy that allows access only to authenticated user subjects.
    /// </summary>
    public const string UserOnly = "subject:user";

    /// <summary>
    /// A policy that allows access only to authenticated service subjects.
    /// </summary>
    public const string ServiceOnly = "subject:service";

    internal const string PermissionPrefix = "permission:";
    internal const string AnyPermissionPrefix = "permission:any:";
    internal const string AllPermissionsPrefix = "permission:all:";
    internal const string RolePrefix = "role:";
    internal const string AnyRolePrefix = "role:any:";
    internal const string AllRolesPrefix = "role:all:";
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
    /// Builds a policy name that requires at least one of the specified permissions.
    /// </summary>
    /// <param name="permissions">The permissions of which at least one must be granted.</param>
    /// <returns>A policy name understood by the Raycynix policy provider.</returns>
    public static string AnyPermission(params string[] permissions)
    {
        return $"{AnyPermissionPrefix}{JoinValues(permissions)}";
    }

    /// <summary>
    /// Builds a policy name that requires all of the specified permissions.
    /// </summary>
    /// <param name="permissions">The permissions that must all be granted.</param>
    /// <returns>A policy name understood by the Raycynix policy provider.</returns>
    public static string AllPermissions(params string[] permissions)
    {
        return $"{AllPermissionsPrefix}{JoinValues(permissions)}";
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
    /// Builds a policy name that requires at least one of the specified roles.
    /// </summary>
    /// <param name="roles">The roles of which at least one must be assigned.</param>
    /// <returns>A policy name understood by the Raycynix policy provider.</returns>
    public static string AnyRole(params string[] roles)
    {
        return $"{AnyRolePrefix}{JoinValues(roles)}";
    }

    /// <summary>
    /// Builds a policy name that requires all of the specified roles.
    /// </summary>
    /// <param name="roles">The roles that must all be assigned.</param>
    /// <returns>A policy name understood by the Raycynix policy provider.</returns>
    public static string AllRoles(params string[] roles)
    {
        return $"{AllRolesPrefix}{JoinValues(roles)}";
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

    /// <summary>
    /// Builds dynamic policy names from the supplied security attributes.
    /// </summary>
    /// <param name="attributes">The attributes that declare authorization requirements.</param>
    /// <returns>The distinct dynamic policy names required by the attributes.</returns>
    public static IReadOnlyCollection<string> FromAttributes(IEnumerable<object> attributes)
    {
        ArgumentNullException.ThrowIfNull(attributes);

        var policies = new List<string>();

        foreach (var attribute in attributes)
        {
            switch (attribute)
            {
                case RequireAuthenticatedSubjectAttribute:
                    policies.Add(Authenticated);
                    break;
                case RequirePermissionAttribute requirePermission:
                    policies.Add(Permission(requirePermission.Permission));
                    break;
                case RequireAnyPermissionAttribute requireAnyPermission when requireAnyPermission.Permissions.Count > 0:
                    policies.Add(AnyPermission(requireAnyPermission.Permissions.ToArray()));
                    break;
                case RequireAllPermissionsAttribute requireAllPermissions when requireAllPermissions.Permissions.Count > 0:
                    policies.Add(AllPermissions(requireAllPermissions.Permissions.ToArray()));
                    break;
                case RequireRoleAttribute requireRole:
                    policies.Add(Role(requireRole.Role));
                    break;
                case RequireAnyRoleAttribute requireAnyRole when requireAnyRole.Roles.Count > 0:
                    policies.Add(AnyRole(requireAnyRole.Roles.ToArray()));
                    break;
                case RequireAllRolesAttribute requireAllRoles when requireAllRoles.Roles.Count > 0:
                    policies.Add(AllRoles(requireAllRoles.Roles.ToArray()));
                    break;
                case RequireSubjectTypeAttribute requireSubjectType:
                    policies.Add(Subject(requireSubjectType.SubjectType));
                    break;
            }
        }

        return policies.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static string JoinValues(IEnumerable<string> values)
    {
        return string.Join("|", values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()));
    }
}

using Microsoft.AspNetCore.Authorization;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization.Requirements;

/// <summary>
/// Represents a requirement that the current subject must have a specific permission.
/// </summary>
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionRequirement"/> class.
    /// </summary>
    /// <param name="permission">The permission required to satisfy the policy.</param>
    public PermissionRequirement(string permission)
    {
        Permission = AuthorizationRequirementValues.Required(permission, nameof(permission));
    }

    /// <summary>
    /// Gets the permission required to satisfy the policy.
    /// </summary>
    public string Permission { get; }
}

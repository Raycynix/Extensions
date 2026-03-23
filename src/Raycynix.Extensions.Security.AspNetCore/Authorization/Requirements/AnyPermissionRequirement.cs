using Microsoft.AspNetCore.Authorization;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization.Requirements;

/// <summary>
/// Represents a requirement that the current subject must have at least one of the specified permissions.
/// </summary>
public sealed class AnyPermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnyPermissionRequirement"/> class.
    /// </summary>
    /// <param name="permissions">The permissions of which at least one must be granted.</param>
    public AnyPermissionRequirement(IReadOnlyCollection<string> permissions)
    {
        Permissions = permissions;
    }

    /// <summary>
    /// Gets the permissions of which at least one must be granted.
    /// </summary>
    public IReadOnlyCollection<string> Permissions { get; }
}

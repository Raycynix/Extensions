using Microsoft.AspNetCore.Authorization;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization.Requirements;

/// <summary>
/// Represents a requirement that the current subject must have all of the specified permissions.
/// </summary>
public sealed class AllPermissionsRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AllPermissionsRequirement"/> class.
    /// </summary>
    /// <param name="permissions">The permissions that must all be granted.</param>
    public AllPermissionsRequirement(IReadOnlyCollection<string> permissions)
    {
        Permissions = permissions;
    }

    /// <summary>
    /// Gets the permissions that must all be granted.
    /// </summary>
    public IReadOnlyCollection<string> Permissions { get; }
}

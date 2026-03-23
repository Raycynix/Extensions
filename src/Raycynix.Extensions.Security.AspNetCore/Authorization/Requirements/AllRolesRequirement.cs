using Microsoft.AspNetCore.Authorization;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization.Requirements;

/// <summary>
/// Represents a requirement that the current subject must have all of the specified roles.
/// </summary>
public sealed class AllRolesRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AllRolesRequirement"/> class.
    /// </summary>
    /// <param name="roles">The roles that must all be assigned.</param>
    public AllRolesRequirement(IReadOnlyCollection<string> roles)
    {
        Roles = roles;
    }

    /// <summary>
    /// Gets the roles that must all be assigned.
    /// </summary>
    public IReadOnlyCollection<string> Roles { get; }
}

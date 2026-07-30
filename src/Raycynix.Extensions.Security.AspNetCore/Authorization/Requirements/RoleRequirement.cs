using Microsoft.AspNetCore.Authorization;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization.Requirements;

/// <summary>
/// Represents a requirement that the current subject must have a specific role.
/// </summary>
public sealed class RoleRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoleRequirement"/> class.
    /// </summary>
    /// <param name="role">The role required to satisfy the policy.</param>
    public RoleRequirement(string role)
    {
        Role = AuthorizationRequirementValues.Required(role, nameof(role));
    }

    /// <summary>
    /// Gets the role required to satisfy the policy.
    /// </summary>
    public string Role { get; }
}

using Microsoft.AspNetCore.Authorization;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization.Requirements;

/// <summary>
/// Represents a requirement that the current subject must have at least one of the specified roles.
/// </summary>
public sealed class AnyRoleRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnyRoleRequirement"/> class.
    /// </summary>
    /// <param name="roles">The roles of which at least one must be assigned.</param>
    public AnyRoleRequirement(IReadOnlyCollection<string> roles)
    {
        Roles = AuthorizationRequirementValues.RequiredMany(roles, nameof(roles));
    }

    /// <summary>
    /// Gets the roles of which at least one must be assigned.
    /// </summary>
    public IReadOnlyCollection<string> Roles { get; }
}

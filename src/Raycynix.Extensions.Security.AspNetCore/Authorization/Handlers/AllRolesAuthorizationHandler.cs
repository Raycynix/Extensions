using Microsoft.AspNetCore.Authorization;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Requirements;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization.Handlers;

/// <summary>
/// Evaluates whether the current subject has all of the required roles.
/// </summary>
public sealed class AllRolesAuthorizationHandler : AuthorizationHandler<AllRolesRequirement>
{
    private readonly ISecurityContext _securityContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllRolesAuthorizationHandler"/> class.
    /// </summary>
    /// <param name="securityContext">The current request security context.</param>
    public AllRolesAuthorizationHandler(ISecurityContext securityContext)
    {
        _securityContext = securityContext;
    }

    /// <inheritdoc />
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AllRolesRequirement requirement)
    {
        if (requirement.Roles.All(requiredRole =>
                _securityContext.Roles.Contains(requiredRole, StringComparer.OrdinalIgnoreCase)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

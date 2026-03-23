using Microsoft.AspNetCore.Authorization;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Requirements;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization.Handlers;

/// <summary>
/// Evaluates whether the current subject has at least one of the required roles.
/// </summary>
public sealed class AnyRoleAuthorizationHandler : AuthorizationHandler<AnyRoleRequirement>
{
    private readonly ISecurityContext _securityContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnyRoleAuthorizationHandler"/> class.
    /// </summary>
    /// <param name="securityContext">The current request security context.</param>
    public AnyRoleAuthorizationHandler(ISecurityContext securityContext)
    {
        _securityContext = securityContext;
    }

    /// <inheritdoc />
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AnyRoleRequirement requirement)
    {
        if (requirement.Roles.Any(requiredRole =>
                _securityContext.Roles.Contains(requiredRole, StringComparer.OrdinalIgnoreCase)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

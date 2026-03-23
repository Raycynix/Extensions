using Microsoft.AspNetCore.Authorization;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Requirements;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization.Handlers;

/// <summary>
/// Evaluates whether the current subject has at least one of the required permissions.
/// </summary>
public sealed class AnyPermissionAuthorizationHandler : AuthorizationHandler<AnyPermissionRequirement>
{
    private readonly ISecurityContext _securityContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnyPermissionAuthorizationHandler"/> class.
    /// </summary>
    /// <param name="securityContext">The current request security context.</param>
    public AnyPermissionAuthorizationHandler(ISecurityContext securityContext)
    {
        _securityContext = securityContext;
    }

    /// <inheritdoc />
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AnyPermissionRequirement requirement)
    {
        if (requirement.Permissions.Any(requiredPermission =>
                _securityContext.Permissions.Contains(requiredPermission, StringComparer.OrdinalIgnoreCase)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

using Microsoft.AspNetCore.Authorization;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Requirements;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization.Handlers;

/// <summary>
/// Evaluates whether the current subject has all of the required permissions.
/// </summary>
public sealed class AllPermissionsAuthorizationHandler : AuthorizationHandler<AllPermissionsRequirement>
{
    private readonly ISecurityContext _securityContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllPermissionsAuthorizationHandler"/> class.
    /// </summary>
    /// <param name="securityContext">The current request security context.</param>
    public AllPermissionsAuthorizationHandler(ISecurityContext securityContext)
    {
        _securityContext = securityContext;
    }

    /// <inheritdoc />
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AllPermissionsRequirement requirement)
    {
        if (requirement.Permissions.All(requiredPermission =>
                _securityContext.Permissions.Contains(requiredPermission, StringComparer.OrdinalIgnoreCase)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

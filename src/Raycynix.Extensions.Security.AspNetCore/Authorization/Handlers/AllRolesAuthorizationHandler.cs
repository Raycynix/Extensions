using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Requirements;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization.Handlers;

/// <summary>
/// Evaluates whether the current subject has all of the required roles.
/// </summary>
public sealed class AllRolesAuthorizationHandler : AuthorizationHandler<AllRolesRequirement>
{
    private readonly ISecurityContext _securityContext;
    private readonly ILogger<AllRolesAuthorizationHandler>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllRolesAuthorizationHandler"/> class.
    /// </summary>
    /// <param name="securityContext">The current request security context.</param>
    /// <param name="logger">The optional logger used for authorization diagnostics.</param>
    public AllRolesAuthorizationHandler(
        ISecurityContext securityContext,
        ILogger<AllRolesAuthorizationHandler>? logger = null)
    {
        _securityContext = securityContext;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AllRolesRequirement requirement)
    {
        var succeeded = requirement.Roles.All(requiredRole =>
            _securityContext.Roles.Contains(requiredRole, StringComparer.OrdinalIgnoreCase));
        _logger?.LogDebug(
            "Evaluated all-roles requirement. Succeeded={Succeeded}, IsAuthenticated={IsAuthenticated}, RequiredRoleCount={RequiredRoleCount}, RoleCount={RoleCount}.",
            succeeded,
            _securityContext.IsAuthenticated,
            requirement.Roles.Count,
            _securityContext.Roles.Count);

        if (succeeded)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
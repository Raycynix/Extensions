using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Requirements;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization.Handlers;

/// <summary>
/// Evaluates whether the current subject has at least one of the required roles.
/// </summary>
public sealed class AnyRoleAuthorizationHandler : AuthorizationHandler<AnyRoleRequirement>
{
    private readonly ISecurityContext _securityContext;
    private readonly ILogger<AnyRoleAuthorizationHandler>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnyRoleAuthorizationHandler"/> class.
    /// </summary>
    /// <param name="securityContext">The current request security context.</param>
    /// <param name="logger">The optional logger used for authorization diagnostics.</param>
    public AnyRoleAuthorizationHandler(
        ISecurityContext securityContext,
        ILogger<AnyRoleAuthorizationHandler>? logger = null)
    {
        _securityContext = securityContext;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AnyRoleRequirement requirement)
    {
        var succeeded = requirement.Roles.Any(requiredRole =>
            _securityContext.Roles.Contains(requiredRole, StringComparer.OrdinalIgnoreCase));
        _logger?.LogDebug(
            "Evaluated any-role requirement. Succeeded={Succeeded}, IsAuthenticated={IsAuthenticated}, RequiredRoleCount={RequiredRoleCount}, RoleCount={RoleCount}.",
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
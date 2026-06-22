using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Requirements;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization.Handlers;

/// <summary>
/// Evaluates role-based authorization requirements against the shared security context.
/// </summary>
public sealed class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
{
    private readonly ISecurityContext _securityContext;
    private readonly ILogger<RoleAuthorizationHandler>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleAuthorizationHandler"/> class.
    /// </summary>
    /// <param name="securityContext">The current request security context.</param>
    /// <param name="logger">The optional logger used for authorization diagnostics.</param>
    public RoleAuthorizationHandler(
        ISecurityContext securityContext,
        ILogger<RoleAuthorizationHandler>? logger = null)
    {
        _securityContext = securityContext;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleRequirement requirement)
    {
        var succeeded = _securityContext.Roles.Contains(requirement.Role, StringComparer.OrdinalIgnoreCase);
        _logger?.LogDebug(
            "Evaluated role requirement. Succeeded={Succeeded}, IsAuthenticated={IsAuthenticated}, RoleCount={RoleCount}.",
            succeeded,
            _securityContext.IsAuthenticated,
            _securityContext.Roles.Count);

        if (succeeded)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
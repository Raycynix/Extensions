using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Requirements;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization.Handlers;

/// <summary>
/// Evaluates whether the current subject has at least one of the required permissions.
/// </summary>
public sealed class AnyPermissionAuthorizationHandler : AuthorizationHandler<AnyPermissionRequirement>
{
    private readonly ISecurityContext _securityContext;
    private readonly ILogger<AnyPermissionAuthorizationHandler>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnyPermissionAuthorizationHandler"/> class.
    /// </summary>
    /// <param name="securityContext">The current request security context.</param>
    /// <param name="logger">The optional logger used for authorization diagnostics.</param>
    public AnyPermissionAuthorizationHandler(
        ISecurityContext securityContext,
        ILogger<AnyPermissionAuthorizationHandler>? logger = null)
    {
        _securityContext = securityContext;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AnyPermissionRequirement requirement)
    {
        var succeeded = requirement.Permissions.Any(requiredPermission =>
            _securityContext.Permissions.Contains(requiredPermission, StringComparer.OrdinalIgnoreCase));
        _logger?.LogDebug(
            "Evaluated any-permission requirement. Succeeded={Succeeded}, IsAuthenticated={IsAuthenticated}, RequiredPermissionCount={RequiredPermissionCount}, PermissionCount={PermissionCount}.",
            succeeded,
            _securityContext.IsAuthenticated,
            requirement.Permissions.Count,
            _securityContext.Permissions.Count);

        if (succeeded)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
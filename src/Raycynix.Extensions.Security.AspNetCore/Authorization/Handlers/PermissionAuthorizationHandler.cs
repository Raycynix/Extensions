using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Requirements;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization.Handlers;

/// <summary>
/// Evaluates permission-based authorization requirements against the shared security context.
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ISecurityContext _securityContext;
    private readonly ILogger<PermissionAuthorizationHandler>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionAuthorizationHandler"/> class.
    /// </summary>
    /// <param name="securityContext">The current request security context.</param>
    /// <param name="logger">The optional logger used for authorization diagnostics.</param>
    public PermissionAuthorizationHandler(
        ISecurityContext securityContext,
        ILogger<PermissionAuthorizationHandler>? logger = null)
    {
        _securityContext = securityContext;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var succeeded = _securityContext.Permissions.Contains(requirement.Permission, StringComparer.OrdinalIgnoreCase);
        _logger?.LogDebug(
            "Evaluated permission requirement. Succeeded={Succeeded}, IsAuthenticated={IsAuthenticated}, PermissionCount={PermissionCount}.",
            succeeded,
            _securityContext.IsAuthenticated,
            _securityContext.Permissions.Count);

        if (succeeded)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
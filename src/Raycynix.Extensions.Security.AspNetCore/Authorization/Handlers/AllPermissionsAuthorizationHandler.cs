using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Requirements;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization.Handlers;

/// <summary>
/// Evaluates whether the current subject has all of the required permissions.
/// </summary>
public sealed class AllPermissionsAuthorizationHandler : AuthorizationHandler<AllPermissionsRequirement>
{
    private readonly ISecurityContext _securityContext;
    private readonly ILogger<AllPermissionsAuthorizationHandler>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllPermissionsAuthorizationHandler"/> class.
    /// </summary>
    /// <param name="securityContext">The current request security context.</param>
    /// <param name="logger">The optional logger used for authorization diagnostics.</param>
    public AllPermissionsAuthorizationHandler(
        ISecurityContext securityContext,
        ILogger<AllPermissionsAuthorizationHandler>? logger = null)
    {
        _securityContext = securityContext;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AllPermissionsRequirement requirement)
    {
        var succeeded = requirement.Permissions.All(requiredPermission =>
            _securityContext.Permissions.Contains(requiredPermission, StringComparer.OrdinalIgnoreCase));
        _logger?.LogDebug(
            "Evaluated all-permissions requirement. Succeeded={Succeeded}, IsAuthenticated={IsAuthenticated}, RequiredPermissionCount={RequiredPermissionCount}, PermissionCount={PermissionCount}.",
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
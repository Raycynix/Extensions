using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Requirements;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization.Handlers;

/// <summary>
/// Evaluates subject-type authorization requirements against the shared security context.
/// </summary>
public sealed class SubjectTypeAuthorizationHandler : AuthorizationHandler<SubjectTypeRequirement>
{
    private readonly ISecurityContext _securityContext;
    private readonly ILogger<SubjectTypeAuthorizationHandler>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubjectTypeAuthorizationHandler"/> class.
    /// </summary>
    /// <param name="securityContext">The current request security context.</param>
    /// <param name="logger">The optional logger used for authorization diagnostics.</param>
    public SubjectTypeAuthorizationHandler(
        ISecurityContext securityContext,
        ILogger<SubjectTypeAuthorizationHandler>? logger = null)
    {
        _securityContext = securityContext;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SubjectTypeRequirement requirement)
    {
        var succeeded = _securityContext.SubjectType == requirement.SubjectType;
        _logger?.LogDebug(
            "Evaluated subject-type requirement. Succeeded={Succeeded}, IsAuthenticated={IsAuthenticated}, SubjectType={SubjectType}.",
            succeeded,
            _securityContext.IsAuthenticated,
            _securityContext.SubjectType);

        if (succeeded)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
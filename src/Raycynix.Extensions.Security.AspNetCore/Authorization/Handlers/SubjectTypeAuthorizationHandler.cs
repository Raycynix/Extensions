using Microsoft.AspNetCore.Authorization;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Requirements;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization.Handlers;

/// <summary>
/// Evaluates subject-type authorization requirements against the shared security context.
/// </summary>
public sealed class SubjectTypeAuthorizationHandler : AuthorizationHandler<SubjectTypeRequirement>
{
    private readonly ISecurityContext _securityContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubjectTypeAuthorizationHandler"/> class.
    /// </summary>
    /// <param name="securityContext">The current request security context.</param>
    public SubjectTypeAuthorizationHandler(ISecurityContext securityContext)
    {
        _securityContext = securityContext;
    }

    /// <inheritdoc />
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SubjectTypeRequirement requirement)
    {
        if (_securityContext.SubjectType == requirement.SubjectType)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

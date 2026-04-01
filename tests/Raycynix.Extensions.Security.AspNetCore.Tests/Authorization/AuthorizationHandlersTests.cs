using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Raycynix.Extensions.Security.Abstractions.Enums;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Handlers;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Requirements;
using Raycynix.Extensions.Security.Implementations;

namespace Raycynix.Extensions.Security.AspNetCore.Tests.Authorization;

/// <summary>
/// Covers direct authorization handler behavior for the shared security context.
/// </summary>
public class AuthorizationHandlersTests
{
    /// <summary>
    /// Verifies that the permission handler succeeds when the required permission is present.
    /// </summary>
    [Fact]
    public async Task PermissionAuthorizationHandler_ShouldSucceedWhenPermissionExists()
    {
        var handler = new PermissionAuthorizationHandler(CreateSecurityContext());
        var requirement = new PermissionRequirement("users.read");
        var context = CreateContext(requirement);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that the any-permission handler succeeds when at least one required permission is present.
    /// </summary>
    [Fact]
    public async Task AnyPermissionAuthorizationHandler_ShouldSucceedWhenAnyPermissionExists()
    {
        var handler = new AnyPermissionAuthorizationHandler(CreateSecurityContext());
        var requirement = new AnyPermissionRequirement(["users.delete", "users.read"]);
        var context = CreateContext(requirement);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that the all-permissions handler succeeds only when all required permissions are present.
    /// </summary>
    [Fact]
    public async Task AllPermissionsAuthorizationHandler_ShouldSucceedWhenAllPermissionsExist()
    {
        var handler = new AllPermissionsAuthorizationHandler(CreateSecurityContext());
        var requirement = new AllPermissionsRequirement(["users.read", "users.update"]);
        var context = CreateContext(requirement);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that the role handler succeeds when the required role is present.
    /// </summary>
    [Fact]
    public async Task RoleAuthorizationHandler_ShouldSucceedWhenRoleExists()
    {
        var handler = new RoleAuthorizationHandler(CreateSecurityContext());
        var requirement = new RoleRequirement("admin");
        var context = CreateContext(requirement);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that the any-role handler succeeds when at least one required role is present.
    /// </summary>
    [Fact]
    public async Task AnyRoleAuthorizationHandler_ShouldSucceedWhenAnyRoleExists()
    {
        var handler = new AnyRoleAuthorizationHandler(CreateSecurityContext());
        var requirement = new AnyRoleRequirement(["auditor", "support"]);
        var context = CreateContext(requirement);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that the all-roles handler succeeds only when all required roles are present.
    /// </summary>
    [Fact]
    public async Task AllRolesAuthorizationHandler_ShouldSucceedWhenAllRolesExist()
    {
        var handler = new AllRolesAuthorizationHandler(CreateSecurityContext());
        var requirement = new AllRolesRequirement(["admin", "support"]);
        var context = CreateContext(requirement);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that the subject-type handler succeeds when the current subject type matches the requirement.
    /// </summary>
    [Fact]
    public async Task SubjectTypeAuthorizationHandler_ShouldSucceedWhenSubjectTypeMatches()
    {
        var handler = new SubjectTypeAuthorizationHandler(CreateSecurityContext());
        var requirement = new SubjectTypeRequirement(SecuritySubjectType.User);
        var context = CreateContext(requirement);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    private static AuthorizationHandlerContext CreateContext(IAuthorizationRequirement requirement)
    {
        return new AuthorizationHandlerContext([requirement], new ClaimsPrincipal(new ClaimsIdentity()), resource: null);
    }

    private static SecurityContext CreateSecurityContext()
    {
        return new SecurityContext
        {
            IsAuthenticated = true,
            SubjectId = "user-1",
            SubjectType = SecuritySubjectType.User,
            Roles = ["admin", "support"],
            Permissions = ["users.read", "users.update"]
        };
    }
}

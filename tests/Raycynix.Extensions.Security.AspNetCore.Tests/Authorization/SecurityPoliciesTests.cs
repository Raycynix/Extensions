using FluentAssertions;
using Raycynix.Extensions.Security.Abstractions.Attributes;
using Raycynix.Extensions.Security.Abstractions.Enums;
using Raycynix.Extensions.Security.AspNetCore.Authorization;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Requirements;

namespace Raycynix.Extensions.Security.AspNetCore.Tests.Authorization;

/// <summary>
/// Covers security policy name generation.
/// </summary>
public class SecurityPoliciesTests
{
    /// <summary>
    /// Verifies that policy helpers generate the expected dynamic policy names.
    /// </summary>
    [Fact]
    public void Helpers_ShouldBuildExpectedPolicyNames()
    {
        SecurityPolicies.Authenticated.Should().Be("authenticated");
        SecurityPolicies.Permission("users.read").Should().Be("permission:users.read");
        SecurityPolicies.AnyPermission("users.read", "users.update").Should().Be("permission:any:users.read|users.update");
        SecurityPolicies.AllPermissions("users.read", "users.update").Should().Be("permission:all:users.read|users.update");
        SecurityPolicies.Role("admin").Should().Be("role:admin");
        SecurityPolicies.AnyRole("admin", "support").Should().Be("role:any:admin|support");
        SecurityPolicies.AllRoles("admin", "support").Should().Be("role:all:admin|support");
        SecurityPolicies.Subject(SecuritySubjectType.Service).Should().Be("subject:service");
    }

    /// <summary>
    /// Verifies that shared security attributes are translated into the expected dynamic policy names.
    /// </summary>
    [Fact]
    public void FromAttributes_ShouldBuildExpectedPolicyNames()
    {
        var policies = SecurityPolicies.FromAttributes(
        [
            new RequireAuthenticatedSubjectAttribute(),
            new RequirePermissionAttribute("users.read"),
            new RequireAnyRoleAttribute("admin", "support"),
            new RequireSubjectTypeAttribute(SecuritySubjectType.Service)
        ]);

        policies.Should().BeEquivalentTo(
        [
            SecurityPolicies.Authenticated,
            SecurityPolicies.Permission("users.read"),
            SecurityPolicies.AnyRole("admin", "support"),
            SecurityPolicies.Subject(SecuritySubjectType.Service)
        ]);
    }

    [Fact]
    public void Permission_ShouldRejectBlankValue()
    {
        var act = () => SecurityPolicies.Permission(" ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AnyPermission_ShouldRejectEmptyValues()
    {
        var act = () => SecurityPolicies.AnyPermission();

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AuthorizationAttributes_ShouldSupportMethods()
    {
        var usage = typeof(RequirePermissionAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), inherit: false)
            .Cast<AttributeUsageAttribute>()
            .Single();

        usage.ValidOn.Should().HaveFlag(AttributeTargets.Method);
    }

    [Fact]
    public void Requirement_ShouldSnapshotAndNormalizeValues()
    {
        var permissions = new List<string> { " users.read ", "USERS.READ" };

        var requirement = new AnyPermissionRequirement(permissions);
        permissions.Add("users.write");

        requirement.Permissions.Should().ContainSingle().Which.Should().Be("users.read");
    }
}

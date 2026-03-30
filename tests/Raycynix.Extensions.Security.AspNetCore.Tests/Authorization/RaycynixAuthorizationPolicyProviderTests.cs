using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Security.Abstractions.Enums;
using Raycynix.Extensions.Security.AspNetCore.Authorization;
using Raycynix.Extensions.Security.AspNetCore.Authorization.PolicyProvider;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Requirements;

namespace Raycynix.Extensions.Security.AspNetCore.Tests.Authorization;

/// <summary>
/// Covers dynamic authorization policy generation.
/// </summary>
public class RaycynixAuthorizationPolicyProviderTests
{
    /// <summary>
    /// Verifies that a permission policy name produces the expected authenticated policy and requirement.
    /// </summary>
    [Fact]
    public async Task GetPolicyAsync_ShouldBuildPermissionPolicy()
    {
        var provider = CreateProvider();

        var policy = await provider.GetPolicyAsync(SecurityPolicies.Permission("users.read"));

        policy.Should().NotBeNull();
        policy!.AuthenticationSchemes.Should().Contain(JwtBearerDefaults.AuthenticationScheme);
        policy.Requirements.Should().ContainSingle(requirement => requirement is PermissionRequirement);
    }

    /// <summary>
    /// Verifies that an all-permissions policy name produces the expected requirement.
    /// </summary>
    [Fact]
    public async Task GetPolicyAsync_ShouldBuildAllPermissionsPolicy()
    {
        var provider = CreateProvider();

        var policy = await provider.GetPolicyAsync(SecurityPolicies.AllPermissions("users.read", "users.update"));

        policy.Should().NotBeNull();
        policy!.Requirements.Should().ContainSingle(requirement => requirement is AllPermissionsRequirement);
    }

    /// <summary>
    /// Verifies that a role policy name produces the expected role requirement.
    /// </summary>
    [Fact]
    public async Task GetPolicyAsync_ShouldBuildRolePolicy()
    {
        var provider = CreateProvider();

        var policy = await provider.GetPolicyAsync(SecurityPolicies.Role("admin"));

        policy.Should().NotBeNull();
        policy!.Requirements.Should().ContainSingle(requirement => requirement is RoleRequirement);
    }

    /// <summary>
    /// Verifies that a subject policy name produces the expected subject type requirement.
    /// </summary>
    [Fact]
    public async Task GetPolicyAsync_ShouldBuildSubjectPolicy()
    {
        var provider = CreateProvider();

        var policy = await provider.GetPolicyAsync(SecurityPolicies.Subject(SecuritySubjectType.Service));

        policy.Should().NotBeNull();
        policy!.Requirements.Should().ContainSingle(requirement => requirement is SubjectTypeRequirement);
    }

    /// <summary>
    /// Verifies that unknown policy names fall back to the default provider behavior.
    /// </summary>
    [Fact]
    public async Task GetPolicyAsync_ShouldReturnNullForUnknownPolicy()
    {
        var provider = CreateProvider();

        var policy = await provider.GetPolicyAsync("unknown-policy");

        policy.Should().BeNull();
    }

    private static RaycynixAuthorizationPolicyProvider CreateProvider()
    {
        return new RaycynixAuthorizationPolicyProvider(Options.Create(new AuthorizationOptions()));
    }
}

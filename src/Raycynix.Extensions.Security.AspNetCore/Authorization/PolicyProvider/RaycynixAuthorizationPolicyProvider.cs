using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Security.Abstractions.Enums;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Requirements;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization.PolicyProvider;

/// <summary>
/// Builds dynamic authorization policies for Raycynix permission, role, and subject-based policy names.
/// </summary>
public sealed class RaycynixAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    private readonly ILogger<RaycynixAuthorizationPolicyProvider>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RaycynixAuthorizationPolicyProvider"/> class.
    /// </summary>
    /// <param name="options">The authorization options.</param>
    /// <param name="logger">The optional logger used for policy resolution diagnostics.</param>
    public RaycynixAuthorizationPolicyProvider(
        IOptions<AuthorizationOptions> options,
        ILogger<RaycynixAuthorizationPolicyProvider>? logger = null)
        : base(options)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var dynamicPolicy = BuildPolicy(policyName);
        if (dynamicPolicy is not null)
        {
            _logger?.LogDebug(
                "Resolved Raycynix dynamic authorization policy. PolicyKind={PolicyKind}, RequirementCount={RequirementCount}.",
                ResolvePolicyKind(policyName),
                dynamicPolicy.Requirements.Count);

            return dynamicPolicy;
        }

        _logger?.LogDebug("Delegating authorization policy lookup to default provider.");
        return await base.GetPolicyAsync(policyName);
    }

    private static string ResolvePolicyKind(string policyName)
    {
        if (string.Equals(policyName, SecurityPolicies.Authenticated, StringComparison.OrdinalIgnoreCase))
        {
            return "Authenticated";
        }

        if (policyName.StartsWith(SecurityPolicies.AnyPermissionPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return "AnyPermission";
        }

        if (policyName.StartsWith(SecurityPolicies.AllPermissionsPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return "AllPermissions";
        }

        if (policyName.StartsWith(SecurityPolicies.PermissionPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return "Permission";
        }

        if (policyName.StartsWith(SecurityPolicies.AnyRolePrefix, StringComparison.OrdinalIgnoreCase))
        {
            return "AnyRole";
        }

        if (policyName.StartsWith(SecurityPolicies.AllRolesPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return "AllRoles";
        }

        if (policyName.StartsWith(SecurityPolicies.RolePrefix, StringComparison.OrdinalIgnoreCase))
        {
            return "Role";
        }

        return policyName.StartsWith(SecurityPolicies.SubjectPrefix, StringComparison.OrdinalIgnoreCase)
            ? "SubjectType"
            : "Unknown";
    }

    private static AuthorizationPolicy? BuildPolicy(string policyName)
    {
        if (string.Equals(policyName, SecurityPolicies.Authenticated, StringComparison.OrdinalIgnoreCase))
        {
            return BuildAuthenticatedPolicy();
        }

        if (policyName.StartsWith(SecurityPolicies.AnyPermissionPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permissions = SplitValues(policyName[SecurityPolicies.AnyPermissionPrefix.Length..]);
            if (permissions.Length > 0)
            {
                return BuildAuthenticatedPolicy(new AnyPermissionRequirement(permissions));
            }
        }

        if (policyName.StartsWith(SecurityPolicies.AllPermissionsPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permissions = SplitValues(policyName[SecurityPolicies.AllPermissionsPrefix.Length..]);
            if (permissions.Length > 0)
            {
                return BuildAuthenticatedPolicy(new AllPermissionsRequirement(permissions));
            }
        }

        if (policyName.StartsWith(SecurityPolicies.PermissionPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permission = policyName[SecurityPolicies.PermissionPrefix.Length..];
            if (!string.IsNullOrWhiteSpace(permission))
            {
                return BuildAuthenticatedPolicy(new PermissionRequirement(permission));
            }
        }

        if (policyName.StartsWith(SecurityPolicies.AnyRolePrefix, StringComparison.OrdinalIgnoreCase))
        {
            var roles = SplitValues(policyName[SecurityPolicies.AnyRolePrefix.Length..]);
            if (roles.Length > 0)
            {
                return BuildAuthenticatedPolicy(new AnyRoleRequirement(roles));
            }
        }

        if (policyName.StartsWith(SecurityPolicies.AllRolesPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var roles = SplitValues(policyName[SecurityPolicies.AllRolesPrefix.Length..]);
            if (roles.Length > 0)
            {
                return BuildAuthenticatedPolicy(new AllRolesRequirement(roles));
            }
        }

        if (policyName.StartsWith(SecurityPolicies.RolePrefix, StringComparison.OrdinalIgnoreCase))
        {
            var role = policyName[SecurityPolicies.RolePrefix.Length..];
            if (!string.IsNullOrWhiteSpace(role))
            {
                return BuildAuthenticatedPolicy(new RoleRequirement(role));
            }
        }

        if (policyName.StartsWith(SecurityPolicies.SubjectPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var subjectTypeValue = policyName[SecurityPolicies.SubjectPrefix.Length..];
            if (Enum.TryParse<SecuritySubjectType>(subjectTypeValue, ignoreCase: true, out var subjectType))
            {
                return BuildAuthenticatedPolicy(new SubjectTypeRequirement(subjectType));
            }
        }

        return null;
    }

    private static AuthorizationPolicy BuildAuthenticatedPolicy(IAuthorizationRequirement requirement)
    {
        return new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .AddRequirements(requirement)
            .Build();
    }

    private static AuthorizationPolicy BuildAuthenticatedPolicy()
    {
        return new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .Build();
    }

    private static string[] SplitValues(string value)
    {
        return value.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
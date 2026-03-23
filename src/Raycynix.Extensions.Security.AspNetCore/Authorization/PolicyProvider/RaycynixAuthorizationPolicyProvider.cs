using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Security.Abstractions.Enums;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Requirements;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization.PolicyProvider;

/// <summary>
/// Builds dynamic authorization policies for Raycynix permission, role, and subject-based policy names.
/// </summary>
public sealed class RaycynixAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RaycynixAuthorizationPolicyProvider"/> class.
    /// </summary>
    /// <param name="options">The authorization options.</param>
    public RaycynixAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    /// <inheritdoc />
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var dynamicPolicy = BuildPolicy(policyName);
        if (dynamicPolicy is not null)
        {
            return dynamicPolicy;
        }

        return await base.GetPolicyAsync(policyName);
    }

    private static AuthorizationPolicy? BuildPolicy(string policyName)
    {
        if (policyName.StartsWith(SecurityPolicies.PermissionPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permission = policyName[SecurityPolicies.PermissionPrefix.Length..];
            if (!string.IsNullOrWhiteSpace(permission))
            {
                return BuildAuthenticatedPolicy(new PermissionRequirement(permission));
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
}

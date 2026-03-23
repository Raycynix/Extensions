using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Raycynix.Extensions.Security.Abstractions.Constants;
using Raycynix.Extensions.Security.Abstractions.Enums;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.Implementation;

namespace Raycynix.Extensions.Security.AspNetCore.Implementation;

internal static class HttpSecurityContextFactory
{
    public static ISecurityContext Create(ClaimsPrincipal? principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            throw new InvalidOperationException(
                "The current request is not authenticated. Ensure JWT authentication is configured and executed before resolving ISecurityContext.");
        }

        var subjectId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrWhiteSpace(subjectId))
        {
            throw new InvalidOperationException("The authenticated principal does not contain the required 'sub' claim.");
        }

        var subjectTypeValue = principal.FindFirst(SecurityClaimTypes.SubjectType)?.Value;
        if (string.IsNullOrWhiteSpace(subjectTypeValue) ||
            !Enum.TryParse<SecuritySubjectType>(subjectTypeValue, ignoreCase: true, out var subjectType))
        {
            throw new InvalidOperationException(
                $"The authenticated principal does not contain a valid '{SecurityClaimTypes.SubjectType}' claim.");
        }

        return new SecurityContext
        {
            IsAuthenticated = true,
            SubjectId = subjectId,
            SubjectType = subjectType,
            Roles = ReadClaimValues(principal, SecurityClaimTypes.Roles),
            Permissions = ReadClaimValues(principal, SecurityClaimTypes.Permissions)
        };
    }

    private static IReadOnlyCollection<string> ReadClaimValues(ClaimsPrincipal principal, string claimType)
    {
        return principal.Claims
            .Where(claim => claim.Type == claimType)
            .SelectMany(claim => SplitClaimValue(claim.Value))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IEnumerable<string> SplitClaimValue(string value)
    {
        return value.Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}

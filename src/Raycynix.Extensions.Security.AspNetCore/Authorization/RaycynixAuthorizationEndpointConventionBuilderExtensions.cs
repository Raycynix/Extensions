using System.Reflection;
using Microsoft.AspNetCore.Builder;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization;

/// <summary>
/// Provides endpoint-builder helpers that apply Raycynix security attributes through standard ASP.NET Core authorization policies.
/// </summary>
public static class RaycynixAuthorizationEndpointConventionBuilderExtensions
{
    /// <summary>
    /// Applies authorization policies derived from the supplied security attributes to the endpoint builder.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint convention builder type.</typeparam>
    /// <param name="builder">The builder to update.</param>
    /// <param name="attributes">The security attributes to translate into policy names.</param>
    /// <returns>The same builder instance.</returns>
    public static TBuilder RequireRaycynixAuthorization<TBuilder>(
        this TBuilder builder,
        params object[] attributes)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(attributes);

        var policies = SecurityPolicies.FromAttributes(attributes);
        foreach (var policy in policies)
        {
            builder.RequireAuthorization(policy);
        }

        return builder;
    }

    /// <summary>
    /// Applies authorization policies derived from the security attributes declared on the supplied member.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint convention builder type.</typeparam>
    /// <param name="builder">The builder to update.</param>
    /// <param name="member">The member whose attributes should be translated into policy names.</param>
    /// <returns>The same builder instance.</returns>
    public static TBuilder RequireRaycynixAuthorization<TBuilder>(
        this TBuilder builder,
        MemberInfo member)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(member);

        return builder.RequireRaycynixAuthorization(member.GetCustomAttributes(inherit: true).ToArray());
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raycynix.Extensions.Security.Abstractions.Constants;
using Raycynix.Extensions.Security.Abstractions.Enums;
using Raycynix.Extensions.Security.Abstractions.Interfaces;

namespace Raycynix.Extensions.Security.AspNetCore.Tests.Context;

/// <summary>
/// Covers security context resolution through the public ASP.NET Core registration API.
/// </summary>
public class SecurityContextResolutionTests
{
    /// <summary>
    /// Verifies that a valid authenticated principal is exposed as the standard scoped security context.
    /// </summary>
    [Fact]
    public void AddRaycynixAspNetCoreSecurity_ShouldResolveSecurityContextFromAuthenticatedPrincipal()
    {
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = CreatePrincipal(
                    new Claim(JwtRegisteredClaimNames.Sub, "user-1"),
                    new Claim(SecurityClaimTypes.SubjectType, SecuritySubjectType.User.ToString()),
                    new Claim(SecurityClaimTypes.Roles, "admin support"),
                    new Claim(SecurityClaimTypes.Permissions, "users.read,users.update"))
            }
        };

        var services = new ServiceCollection();
        services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);
        services.AddRaycynixAspNetCoreSecurity(CreateValidConfiguration());

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ISecurityContext>();

        context.IsAuthenticated.Should().BeTrue();
        context.SubjectId.Should().Be("user-1");
        context.SubjectType.Should().Be(SecuritySubjectType.User);
        context.Roles.Should().BeEquivalentTo(["admin", "support"]);
        context.Permissions.Should().BeEquivalentTo(["users.read", "users.update"]);
    }

    /// <summary>
    /// Verifies that unauthenticated requests resolve to an anonymous security context.
    /// </summary>
    [Fact]
    public void AddRaycynixAspNetCoreSecurity_ShouldResolveAnonymousSecurityContext_ForUnauthenticatedPrincipal()
    {
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            }
        };

        var services = new ServiceCollection();
        services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);
        services.AddRaycynixAspNetCoreSecurity(CreateValidConfiguration());

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ISecurityContext>();

        context.IsAuthenticated.Should().BeFalse();
        context.SubjectId.Should().BeEmpty();
        context.Roles.Should().BeEmpty();
        context.Permissions.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that incomplete authenticated principals fail closed without breaking request resolution.
    /// </summary>
    [Fact]
    public void AddRaycynixAspNetCoreSecurity_ShouldResolveAnonymousContext_WhenSubjectInformationIsMissing()
    {
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = CreatePrincipal(
                    new Claim(SecurityClaimTypes.SubjectType, SecuritySubjectType.User.ToString()))
            }
        };

        var services = new ServiceCollection();
        services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);
        services.AddRaycynixAspNetCoreSecurity(CreateValidConfiguration());

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ISecurityContext>();

        context.IsAuthenticated.Should().BeFalse();
        context.SubjectId.Should().BeEmpty();
        context.Roles.Should().BeEmpty();
        context.Permissions.Should().BeEmpty();
    }

    [Fact]
    public void AddRaycynixAspNetCoreSecurity_ShouldResolveAnonymousContext_WhenSubjectTypeIsInvalid()
    {
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = CreatePrincipal(
                    new Claim(JwtRegisteredClaimNames.Sub, "user-1"),
                    new Claim(SecurityClaimTypes.SubjectType, "invalid"))
            }
        };

        var services = new ServiceCollection();
        services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);
        services.AddRaycynixAspNetCoreSecurity(CreateValidConfiguration());

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ISecurityContext>();

        context.IsAuthenticated.Should().BeFalse();
        context.SubjectId.Should().BeEmpty();
    }

    private static IConfiguration CreateValidConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SecurityOptions:JwtOptions:Authority"] = "https://auth.raycynix.local",
                ["SecurityOptions:JwtOptions:Issuer"] = "raycynix-auth",
                ["SecurityOptions:JwtOptions:Audience"] = "raycynix-services",
                ["SecurityOptions:JwtOptions:AccessTokenLifetime"] = "00:15:00",
                ["SecurityOptions:JwtOptions:RefreshTokenLifetime"] = "14.00:00:00",
                ["SecurityOptions:JwtOptions:ClockSkew"] = "00:01:00"
            })
            .Build();
    }

    private static ClaimsPrincipal CreatePrincipal(params Claim[] claims)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "Bearer"));
    }
}

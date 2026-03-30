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
    /// Verifies that unauthenticated requests are rejected when resolving the scoped security context.
    /// </summary>
    [Fact]
    public void AddRaycynixAspNetCoreSecurity_ShouldRejectUnauthenticatedPrincipal()
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

        var action = () => scope.ServiceProvider.GetRequiredService<ISecurityContext>();

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*The current request is not authenticated.*");
    }

    /// <summary>
    /// Verifies that missing subject information is rejected when resolving the scoped security context.
    /// </summary>
    [Fact]
    public void AddRaycynixAspNetCoreSecurity_ShouldRejectMissingSubjectInformation()
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

        var action = () => scope.ServiceProvider.GetRequiredService<ISecurityContext>();

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*does not contain the required 'sub' claim*");
    }

    private static IConfiguration CreateValidConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SecurityConfiguration:Jwt:Authority"] = "https://auth.raycynix.local",
                ["SecurityConfiguration:Jwt:Issuer"] = "raycynix-auth",
                ["SecurityConfiguration:Jwt:Audience"] = "raycynix-services",
                ["SecurityConfiguration:Jwt:AccessTokenLifetime"] = "00:15:00",
                ["SecurityConfiguration:Jwt:RefreshTokenLifetime"] = "14.00:00:00",
                ["SecurityConfiguration:Jwt:ClockSkew"] = "00:01:00"
            })
            .Build();
    }

    private static ClaimsPrincipal CreatePrincipal(params Claim[] claims)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "Bearer"));
    }
}

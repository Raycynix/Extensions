using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Raycynix.Extensions.Security.AspNetCore.Tests.Registration;

/// <summary>
/// Covers ASP.NET Core security registration behavior.
/// </summary>
public class SecurityAspNetCoreRegistrationTests
{
    /// <summary>
    /// Verifies that ASP.NET Core security registration rejects a configuration without JWT authority.
    /// </summary>
    [Fact]
    public void AddRaycynixAspNetCoreSecurity_ShouldRejectMissingAuthority()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SecurityConfiguration:Jwt:Issuer"] = "raycynix-auth",
                ["SecurityConfiguration:Jwt:Audience"] = "raycynix-services",
                ["SecurityConfiguration:Jwt:AccessTokenLifetime"] = "00:15:00",
                ["SecurityConfiguration:Jwt:RefreshTokenLifetime"] = "14.00:00:00",
                ["SecurityConfiguration:Jwt:ClockSkew"] = "00:01:00"
            })
            .Build();

        var action = () => services.AddRaycynixAspNetCoreSecurity(configuration);
        
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*SecurityConfiguration.Jwt.Authority must be provided*");
    }

    /// <summary>
    /// Verifies that ASP.NET Core security registration accepts a valid configuration.
    /// </summary>
    [Fact]
    public void AddRaycynixAspNetCoreSecurity_ShouldRegisterWithValidConfiguration()
    {
        var services = new ServiceCollection();

        var action = () => services.AddRaycynixAspNetCoreSecurity(CreateValidConfiguration());

        action.Should().NotThrow();
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
}

using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Conventions;

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

    /// <summary>
    /// Verifies that ASP.NET Core security registration configures the shared MVC authorization convention.
    /// </summary>
    [Fact]
    public void AddRaycynixAspNetCoreSecurity_ShouldRegisterMvcAuthorizationConvention()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddRaycynixAspNetCoreSecurity(CreateValidConfiguration());

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<MvcOptions>>().Value;

        options.Conventions.Should().ContainSingle(convention => convention is RaycynixAuthorizationApplicationModelConvention);
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

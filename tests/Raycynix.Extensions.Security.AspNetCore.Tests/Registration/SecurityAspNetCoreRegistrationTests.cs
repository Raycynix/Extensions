using Microsoft.AspNetCore.Authentication.JwtBearer;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.AspNetCore.Authorization.Conventions;
using Raycynix.Extensions.Security.Options;

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
    public void AddRaycynixAspNetCoreSecurity_ShouldRejectMissingAuthority_WhenOptionsAreResolved()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SecurityOptions:JwtOptions:Issuer"] = "raycynix-auth",
                ["SecurityOptions:JwtOptions:Audience"] = "raycynix-services",
                ["SecurityOptions:JwtOptions:AccessTokenLifetime"] = "00:15:00",
                ["SecurityOptions:JwtOptions:RefreshTokenLifetime"] = "14.00:00:00",
                ["SecurityOptions:JwtOptions:ClockSkew"] = "00:01:00"
            })
            .Build();

        services.AddRaycynixAspNetCoreSecurity(configuration);
        using var provider = services.BuildServiceProvider();

        var action = () => provider.GetRequiredService<SecurityOptions>();

        action.Should().Throw<OptionsValidationException>()
            .WithMessage("*SecurityOptions.JwtOptions.Authority must be provided*");
    }

    /// <summary>
    /// Verifies that a missing nested JWT options object produces validation errors instead of a null reference.
    /// </summary>
    [Fact]
    public void AddRaycynixAspNetCoreSecurity_ShouldRejectMissingJwtOptions_WhenOptionsAreResolved()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddRaycynixAspNetCoreSecurity(
            configuration,
            options => options.JwtOptions = null!);
        using var provider = services.BuildServiceProvider();

        var action = () => provider.GetRequiredService<SecurityOptions>();

        action.Should().Throw<OptionsValidationException>()
            .WithMessage("*JWT options must be provided*")
            .WithMessage("*SecurityOptions.JwtOptions.Authority must be provided*");
    }

    /// <summary>
    /// Verifies that ASP.NET Core security registration accepts a valid configuration.
    /// </summary>
    [Fact]
    public void AddRaycynixAspNetCoreSecurity_ShouldRegisterWithValidConfiguration()
    {
        var services = new ServiceCollection();

        services.AddRaycynixAspNetCoreSecurity(CreateValidConfiguration());
        using var provider = services.BuildServiceProvider();

        var security = provider.GetRequiredService<SecurityOptions>();
        var jwt = provider.GetRequiredService<JwtOptions>();
        var bearer = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        jwt.Should().BeSameAs(security.JwtOptions);
        jwt.Authority.Should().Be("https://auth.raycynix.local");
        bearer.Authority.Should().Be(jwt.Authority);
        bearer.TokenValidationParameters.ValidIssuer.Should().Be(jwt.Issuer);
        bearer.TokenValidationParameters.ValidAudience.Should().Be(jwt.Audience);
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

    /// <summary>
    /// Verifies that ASP.NET Core security registration exposes a request security context implementation.
    /// </summary>
    [Fact]
    public void AddRaycynixAspNetCoreSecurity_ShouldRegisterScopedSecurityContext()
    {
        var services = new ServiceCollection();
        services.AddRaycynixAspNetCoreSecurity(CreateValidConfiguration());

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ISecurityContext>();

        context.Should().NotBeNull();
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
}

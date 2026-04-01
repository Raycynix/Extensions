using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.Configurations;
using Raycynix.Extensions.Security.Implementations;

namespace Raycynix.Extensions.Security.Tests.Registration;

/// <summary>
/// Covers core security registration behavior.
/// </summary>
public class SecurityRegistrationTests
{
    /// <summary>
    /// Verifies that core security registration without configuration adds the default scoped security context.
    /// </summary>
    [Fact]
    public void AddRaycynixSecurity_ShouldRegisterDefaultSecurityContext()
    {
        var services = new ServiceCollection();
        services.AddRaycynixSecurity();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ISecurityContext>();

        context.Should().NotBeNull();
        context.Should().BeOfType<SecurityContext>();
    }

    /// <summary>
    /// Verifies that configured security registration binds the security settings and exposes them through the configuration accessor.
    /// </summary>
    [Fact]
    public void AddRaycynixSecurity_ShouldBindSecurityConfiguration()
    {
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

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRaycynixSecurity(configuration);

        using var provider = services.BuildServiceProvider();
        var accessor = provider.GetRequiredService<IConfigurationAccessor<SecurityConfiguration>>();
        var snapshot = provider.GetRequiredService<SecurityConfiguration>();

        accessor.Current.Jwt.Issuer.Should().Be("raycynix-auth");
        accessor.Current.Jwt.Audience.Should().Be("raycynix-services");
        snapshot.Jwt.Issuer.Should().Be("raycynix-auth");
        snapshot.Jwt.Audience.Should().Be("raycynix-services");
    }

    /// <summary>
    /// Verifies that the setup callback runs after binding and can override the bound security settings.
    /// </summary>
    [Fact]
    public void AddRaycynixSecurity_ShouldApplySetupCallbackAfterBinding()
    {
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

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRaycynixSecurity(configuration, setup =>
        {
            setup.Jwt.Audience = "overridden-audience";
            setup.Jwt.RequireHttpsMetadata = false;
        });

        using var provider = services.BuildServiceProvider();
        var snapshot = provider.GetRequiredService<SecurityConfiguration>();

        snapshot.Jwt.Audience.Should().Be("overridden-audience");
        snapshot.Jwt.RequireHttpsMetadata.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that omitted JWT values keep their default values after configuration binding.
    /// </summary>
    [Fact]
    public void AddRaycynixSecurity_ShouldPreserveJwtDefaultsForOmittedValues()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SecurityConfiguration:Jwt:Issuer"] = "raycynix-auth",
                ["SecurityConfiguration:Jwt:Audience"] = "raycynix-services"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRaycynixSecurity(configuration);

        using var provider = services.BuildServiceProvider();
        var snapshot = provider.GetRequiredService<SecurityConfiguration>();

        snapshot.Jwt.AccessTokenLifetime.Should().Be(TimeSpan.FromMinutes(15));
        snapshot.Jwt.RefreshTokenLifetime.Should().Be(TimeSpan.FromDays(14));
        snapshot.Jwt.ClockSkew.Should().Be(TimeSpan.FromMinutes(1));
        snapshot.Jwt.RequireHttpsMetadata.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that configured security registration fails when the bound security settings are invalid.
    /// </summary>
    [Fact]
    public void AddRaycynixSecurity_ShouldRejectInvalidSecurityConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SecurityConfiguration:Jwt:Issuer"] = string.Empty,
                ["SecurityConfiguration:Jwt:Audience"] = "raycynix-services"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRaycynixSecurity(configuration);

        using var provider = services.BuildServiceProvider();

        var action = () => provider.GetRequiredService<IOptions<SecurityConfiguration>>().Value;

        action.Should().Throw<OptionsValidationException>()
            .WithMessage("*JWT issuer must be provided.*");
    }
}

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.Implementations;
using Raycynix.Extensions.Security.Options;

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
    public void AddRaycynixSecurity_ShouldBindSecurityOptionsAndJwtOptions()
    {
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

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRaycynixSecurity(configuration);

        using var provider = services.BuildServiceProvider();
        var accessor = provider.GetRequiredService<IConfigurationAccessor<SecurityOptions>>();
        var snapshot = provider.GetRequiredService<SecurityOptions>();
        var jwt = provider.GetRequiredService<JwtOptions>();

        accessor.Current.JwtOptions.Issuer.Should().Be("raycynix-auth");
        accessor.Current.JwtOptions.Audience.Should().Be("raycynix-services");
        snapshot.JwtOptions.Issuer.Should().Be("raycynix-auth");
        snapshot.JwtOptions.Audience.Should().Be("raycynix-services");
        jwt.Should().BeSameAs(snapshot.JwtOptions);
        jwt.Issuer.Should().Be("raycynix-auth");
        jwt.Audience.Should().Be("raycynix-services");
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
                ["SecurityOptions:JwtOptions:Issuer"] = "raycynix-auth",
                ["SecurityOptions:JwtOptions:Audience"] = "raycynix-services",
                ["SecurityOptions:JwtOptions:AccessTokenLifetime"] = "00:15:00",
                ["SecurityOptions:JwtOptions:RefreshTokenLifetime"] = "14.00:00:00",
                ["SecurityOptions:JwtOptions:ClockSkew"] = "00:01:00"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRaycynixSecurity(configuration, setup =>
        {
            setup.JwtOptions.Audience = "overridden-audience";
            setup.JwtOptions.RequireHttpsMetadata = false;
        });

        using var provider = services.BuildServiceProvider();
        var snapshot = provider.GetRequiredService<SecurityOptions>();

        snapshot.JwtOptions.Audience.Should().Be("overridden-audience");
        snapshot.JwtOptions.RequireHttpsMetadata.Should().BeFalse();
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
                ["SecurityOptions:JwtOptions:Issuer"] = "raycynix-auth",
                ["SecurityOptions:JwtOptions:Audience"] = "raycynix-services"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRaycynixSecurity(configuration);

        using var provider = services.BuildServiceProvider();
        var snapshot = provider.GetRequiredService<SecurityOptions>();

        snapshot.JwtOptions.AccessTokenLifetime.Should().Be(TimeSpan.FromMinutes(15));
        snapshot.JwtOptions.RefreshTokenLifetime.Should().Be(TimeSpan.FromDays(14));
        snapshot.JwtOptions.ClockSkew.Should().Be(TimeSpan.FromMinutes(1));
        snapshot.JwtOptions.RequireHttpsMetadata.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that configured security registration fails when the bound security settings are invalid.
    /// </summary>
    [Fact]
    public void AddRaycynixSecurity_ShouldRejectInvalidSecurityOptions()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SecurityOptions:JwtOptions:Issuer"] = string.Empty,
                ["SecurityOptions:JwtOptions:Audience"] = "raycynix-services"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRaycynixSecurity(configuration);

        using var provider = services.BuildServiceProvider();

        var action = () => provider.GetRequiredService<IOptions<SecurityOptions>>().Value;

        action.Should().Throw<OptionsValidationException>()
            .WithMessage("*JWT issuer must be provided.*");
    }

    /// <summary>
    /// Verifies that repeated core security registration does not duplicate the default security context descriptor.
    /// </summary>
    [Fact]
    public void AddRaycynixSecurity_ShouldBeIdempotent()
    {
        var services = new ServiceCollection();

        services.AddRaycynixSecurity();
        services.AddRaycynixSecurity();

        services.Count(service => service.ServiceType == typeof(ISecurityContext)).Should().Be(1);
    }
}

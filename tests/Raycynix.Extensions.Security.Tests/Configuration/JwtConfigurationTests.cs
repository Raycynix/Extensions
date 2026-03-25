using FluentAssertions;
using Raycynix.Extensions.Security.Configurations;

namespace Raycynix.Extensions.Security.Tests.Configuration;

/// <summary>
/// Covers validation behavior for JWT configuration.
/// </summary>
public class JwtConfigurationTests
{
    /// <summary>
    /// Verifies that the JWT configuration starts with the expected package defaults.
    /// </summary>
    [Fact]
    public void Constructor_ShouldExposeExpectedDefaults()
    {
        var configuration = new JwtConfiguration();

        configuration.AccessTokenLifetime.Should().Be(TimeSpan.FromMinutes(15));
        configuration.RefreshTokenLifetime.Should().Be(TimeSpan.FromDays(14));
        configuration.ClockSkew.Should().Be(TimeSpan.FromMinutes(1));
        configuration.RequireHttpsMetadata.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that JWT configuration validation succeeds for a valid setup.
    /// </summary>
    [Fact]
    public void Validate_ShouldSucceedForValidConfiguration()
    {
        var configuration = CreateValidJwtConfiguration();

        var action = () => configuration.Validate();

        action.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that JWT configuration validation rejects a missing issuer.
    /// </summary>
    [Fact]
    public void Validate_ShouldRejectMissingIssuer()
    {
        var configuration = CreateValidJwtConfiguration();
        configuration.Issuer = string.Empty;

        var action = () => configuration.Validate();

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*JWT issuer must be provided.*");
    }

    /// <summary>
    /// Verifies that JWT configuration validation rejects a missing audience.
    /// </summary>
    [Fact]
    public void Validate_ShouldRejectMissingAudience()
    {
        var configuration = CreateValidJwtConfiguration();
        configuration.Audience = string.Empty;

        var action = () => configuration.Validate();

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*JWT audience must be provided.*");
    }

    /// <summary>
    /// Verifies that JWT configuration validation rejects a non-positive access token lifetime.
    /// </summary>
    [Fact]
    public void Validate_ShouldRejectNonPositiveAccessTokenLifetime()
    {
        var configuration = CreateValidJwtConfiguration();
        configuration.AccessTokenLifetime = TimeSpan.Zero;

        var action = () => configuration.Validate();

        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Access token lifetime must be greater than zero.*");
    }

    /// <summary>
    /// Verifies that JWT configuration validation rejects a non-positive refresh token lifetime.
    /// </summary>
    [Fact]
    public void Validate_ShouldRejectNonPositiveRefreshTokenLifetime()
    {
        var configuration = CreateValidJwtConfiguration();
        configuration.RefreshTokenLifetime = TimeSpan.Zero;

        var action = () => configuration.Validate();

        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Refresh token lifetime must be greater than zero.*");
    }

    /// <summary>
    /// Verifies that JWT configuration validation rejects a negative clock skew.
    /// </summary>
    [Fact]
    public void Validate_ShouldRejectNegativeClockSkew()
    {
        var configuration = CreateValidJwtConfiguration();
        configuration.ClockSkew = TimeSpan.FromSeconds(-1);

        var action = () => configuration.Validate();

        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Clock skew cannot be negative.*");
    }

    private static JwtConfiguration CreateValidJwtConfiguration()
    {
        return new JwtConfiguration
        {
            Issuer = "raycynix-auth",
            Audience = "raycynix-services",
            AccessTokenLifetime = TimeSpan.FromMinutes(15),
            RefreshTokenLifetime = TimeSpan.FromDays(14),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    }
}

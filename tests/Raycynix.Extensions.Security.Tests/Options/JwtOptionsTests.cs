using FluentAssertions;
using Raycynix.Extensions.Security.Options;

namespace Raycynix.Extensions.Security.Tests.Options;

/// <summary>
/// Covers validation behavior for JWT configuration.
/// </summary>
public class JwtOptionsTests
{
    /// <summary>
    /// Verifies that the JWT configuration starts with the expected package defaults.
    /// </summary>
    [Fact]
    public void Constructor_ShouldExposeExpectedDefaults()
    {
        var configuration = new JwtOptions();

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
        var configuration = CreateValidJwtOptions();

        var action = () => configuration.Validate();

        action.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that JWT configuration validation rejects a missing issuer.
    /// </summary>
    [Fact]
    public void Validate_ShouldRejectMissingIssuer()
    {
        var configuration = CreateValidJwtOptions();
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
        var configuration = CreateValidJwtOptions();
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
        var configuration = CreateValidJwtOptions();
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
        var configuration = CreateValidJwtOptions();
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
        var configuration = CreateValidJwtOptions();
        configuration.ClockSkew = TimeSpan.FromSeconds(-1);

        var action = () => configuration.Validate();

        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Clock skew cannot be negative.*");
    }

    [Fact]
    public void Validate_ShouldRejectRelativeAuthority()
    {
        var configuration = CreateValidJwtOptions();
        configuration.Authority = "/identity";

        var action = () => configuration.Validate();

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*absolute HTTP or HTTPS URI*");
    }

    [Fact]
    public void Validate_ShouldRejectHttpAuthority_WhenHttpsMetadataIsRequired()
    {
        var configuration = CreateValidJwtOptions();
        configuration.Authority = "http://auth.raycynix.local";
        configuration.RequireHttpsMetadata = true;

        var action = () => configuration.Validate();

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*must use HTTPS*");
    }

    private static JwtOptions CreateValidJwtOptions()
    {
        return new JwtOptions
        {
            Issuer = "raycynix-auth",
            Audience = "raycynix-services",
            AccessTokenLifetime = TimeSpan.FromMinutes(15),
            RefreshTokenLifetime = TimeSpan.FromDays(14),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    }
}

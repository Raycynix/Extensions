using FluentAssertions;
using Raycynix.Extensions.Security.Configurations;

namespace Raycynix.Extensions.Security.Tests.Configuration;

/// <summary>
/// Covers validation behavior for the root security configuration.
/// </summary>
public class SecurityConfigurationTests
{
    /// <summary>
    /// Verifies that the root security configuration starts with a non-null JWT section.
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeJwtSection()
    {
        var configuration = new SecurityConfiguration();

        configuration.Jwt.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that the root security configuration delegates validation to the JWT settings.
    /// </summary>
    [Fact]
    public void Validate_ShouldDelegateToJwtValidation()
    {
        var configuration = new SecurityConfiguration
        {
            Jwt = new JwtConfiguration
            {
                Issuer = string.Empty,
                Audience = "raycynix-services"
            }
        };

        var action = () => configuration.Validate();

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*JWT issuer must be provided.*");
    }

}

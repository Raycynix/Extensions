using FluentAssertions;
using Raycynix.Extensions.Security.Options;

namespace Raycynix.Extensions.Security.Tests.Options;

/// <summary>
/// Covers validation behavior for the root security configuration.
/// </summary>
public class SecurityOptionsTests
{
    /// <summary>
    /// Verifies that the root security configuration starts with a non-null JWT section.
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeJwtSection()
    {
        var configuration = new SecurityOptions();

        configuration.JwtOptions.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that the root security configuration delegates validation to the JWT settings.
    /// </summary>
    [Fact]
    public void Validate_ShouldDelegateToJwtValidation()
    {
        var configuration = new SecurityOptions
        {
            JwtOptions = new JwtOptions
            {
                Issuer = string.Empty,
                Audience = "raycynix-services"
            }
        };

        var action = () => configuration.Validate();

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*JWT issuer must be provided.*");
    }

    [Fact]
    public void Validate_ShouldRejectMissingJwtOptions()
    {
        var options = new SecurityOptions
        {
            JwtOptions = null!
        };

        var action = () => options.Validate();

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*JWT options must be provided.*");
    }

}

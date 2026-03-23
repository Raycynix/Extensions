namespace Raycynix.Extensions.Security.Configurations;

/// <summary>
/// Represents the root security settings used by Raycynix security packages.
/// </summary>
public class SecurityConfiguration
{
    /// <summary>
    /// Gets or sets the JWT authentication settings.
    /// </summary>
    public JwtConfiguration Jwt { get; set; } = new();

    /// <summary>
    /// Validates the security configuration and throws when invalid values are provided.
    /// </summary>
    public void Validate()
    {
        Jwt.Validate();
    }
}

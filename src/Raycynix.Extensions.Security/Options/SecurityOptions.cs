namespace Raycynix.Extensions.Security.Options;

/// <summary>
/// Represents the root security settings used by Raycynix security packages.
/// </summary>
public sealed class SecurityOptions
{
    /// <summary>
    /// Gets or sets the JWT authentication settings.
    /// </summary>
    public JwtOptions JwtOptions { get; set; } = new();

    /// <summary>
    /// Validates the security configuration and throws when invalid values are provided.
    /// </summary>
    public void Validate()
    {
        if (JwtOptions is null)
        {
            throw new InvalidOperationException("JWT options must be provided.");
        }

        JwtOptions.Validate();
    }
}

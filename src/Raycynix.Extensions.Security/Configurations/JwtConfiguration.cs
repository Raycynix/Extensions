namespace Raycynix.Extensions.Security.Configurations;

/// <summary>
/// Represents the shared JWT settings used by Raycynix security packages.
/// </summary>
public class JwtConfiguration
{
    /// <summary>
    /// Gets or sets the OpenID Connect or token authority used by ASP.NET Core JWT validation.
    /// </summary>
    public string? Authority { get; set; }

    /// <summary>
    /// Gets or sets the expected token issuer.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expected token audience.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default access token lifetime.
    /// </summary>
    public TimeSpan AccessTokenLifetime { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Gets or sets the default refresh token lifetime.
    /// </summary>
    public TimeSpan RefreshTokenLifetime { get; set; } = TimeSpan.FromDays(14);

    /// <summary>
    /// Gets or sets a value indicating whether HTTPS metadata is required when resolving authority metadata.
    /// </summary>
    public bool RequireHttpsMetadata { get; set; } = true;

    /// <summary>
    /// Gets or sets the acceptable clock skew used during token lifetime validation.
    /// </summary>
    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Validates the JWT configuration and throws when invalid values are provided.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Issuer))
        {
            throw new InvalidOperationException("JWT issuer must be provided.");
        }

        if (string.IsNullOrWhiteSpace(Audience))
        {
            throw new InvalidOperationException("JWT audience must be provided.");
        }

        if (AccessTokenLifetime <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(AccessTokenLifetime),
                "Access token lifetime must be greater than zero.");
        }

        if (RefreshTokenLifetime <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(RefreshTokenLifetime),
                "Refresh token lifetime must be greater than zero.");
        }

        if (ClockSkew < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(ClockSkew), "Clock skew cannot be negative.");
        }
    }
}

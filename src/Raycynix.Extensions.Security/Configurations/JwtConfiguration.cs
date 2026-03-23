namespace Raycynix.Extensions.Security.Configurations;

public class JwtConfiguration
{
    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public TimeSpan AccessTokenLifetime { get; set; } = TimeSpan.FromMinutes(15);

    public TimeSpan RefreshTokenLifetime { get; set; } = TimeSpan.FromDays(14);

    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(1);
}
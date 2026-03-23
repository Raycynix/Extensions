namespace Raycynix.Extensions.Security.Configurations;

public class SecurityConfiguration
{
    public JwtConfiguration Jwt { get; set; } = new();
}
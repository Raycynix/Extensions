using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Security;
using Raycynix.Extensions.Security.Configurations;

Environment.CurrentDirectory = AppContext.BaseDirectory;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRaycynixSecurity(builder.Configuration, options =>
{
    options.Jwt.AccessTokenLifetime = TimeSpan.FromMinutes(30);
    options.Jwt.ClockSkew = TimeSpan.FromSeconds(30);
});

using var host = builder.Build();
using var scope = host.Services.CreateScope();

var securityConfiguration = scope.ServiceProvider.GetRequiredService<SecurityConfiguration>();

Console.WriteLine("Raycynix Security example");
Console.WriteLine($"JWT issuer: {securityConfiguration.Jwt.Issuer}");
Console.WriteLine($"JWT audience: {securityConfiguration.Jwt.Audience}");
Console.WriteLine($"JWT authority: {securityConfiguration.Jwt.Authority}");
Console.WriteLine($"Access token lifetime: {securityConfiguration.Jwt.AccessTokenLifetime}");
Console.WriteLine($"Refresh token lifetime: {securityConfiguration.Jwt.RefreshTokenLifetime}");
Console.WriteLine($"Clock skew: {securityConfiguration.Jwt.ClockSkew}");
Console.WriteLine($"Require HTTPS metadata: {securityConfiguration.Jwt.RequireHttpsMetadata}");

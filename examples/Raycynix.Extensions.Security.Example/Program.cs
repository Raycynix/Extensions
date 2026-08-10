using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Security;
using Raycynix.Extensions.Security.Options;

Environment.CurrentDirectory = AppContext.BaseDirectory;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRaycynixSecurity(builder.Configuration, options =>
{
    options.JwtOptions.AccessTokenLifetime = TimeSpan.FromMinutes(30);
    options.JwtOptions.ClockSkew = TimeSpan.FromSeconds(30);
});

using var host = builder.Build();
using var scope = host.Services.CreateScope();

var securityOptions = scope.ServiceProvider.GetRequiredService<SecurityOptions>();
var jwtOptions = scope.ServiceProvider.GetRequiredService<JwtOptions>();

Console.WriteLine("Raycynix Security example");
Console.WriteLine($"JWT issuer: {jwtOptions.Issuer}");
Console.WriteLine($"JWT audience: {jwtOptions.Audience}");
Console.WriteLine($"JWT authority: {jwtOptions.Authority}");
Console.WriteLine($"Access token lifetime: {jwtOptions.AccessTokenLifetime}");
Console.WriteLine($"Refresh token lifetime: {jwtOptions.RefreshTokenLifetime}");
Console.WriteLine($"Clock skew: {jwtOptions.ClockSkew}");
Console.WriteLine($"Require HTTPS metadata: {jwtOptions.RequireHttpsMetadata}");
Console.WriteLine($"Uses root JWT snapshot: {ReferenceEquals(securityOptions.JwtOptions, jwtOptions)}");
